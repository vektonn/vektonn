using System;
using System.Diagnostics;
using System.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Vektonn.Service.IndexShard;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vektonn.Service
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine($"Vektonn environment variables:{EnvironmentVariables.GetAll(name => name.StartsWith("SH_")).ToPrettyJson()}");

            var hostBuilder = Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    builder => builder
                        .ConfigureServices(
                            services =>
                            {
                                services.AddSingleton<ILog>(new SynchronousConsoleLog());
                                services.AddSingleton<IndexShardBuilder>();
                                services.AddSingleton(
                                    s =>
                                    {
                                        LogMemoryUsage("Before BuildIndexShard()");
                                        var indexShardAccessor = s.GetRequiredService<IndexShardBuilder>().BuildIndexShard();
                                        LogMemoryUsage("After BuildIndexShard()");
                                        return indexShardAccessor;
                                    });

                                services
                                    .AddControllers()
                                    .AddJsonOptions(jsonOptions => HttpJson.Configure(jsonOptions.JsonSerializerOptions));

                                services.AddSwaggerGen(
                                    swaggerGenOptions =>
                                    {
                                        swaggerGenOptions.SupportNonNullableReferenceTypes();
                                        swaggerGenOptions.UseOneOfForPolymorphism();
                                        swaggerGenOptions.SwaggerDoc(
                                            "v1",
                                            new OpenApiInfo
                                            {
                                                Title = "Vektonn API V1",
                                                Version = "v1",
                                                License = new OpenApiLicense
                                                {
                                                    Name = "Apache 2.0",
                                                    Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
                                                }
                                            });
                                    });
                            })
                        .Configure(
                            app =>
                            {
                                app.UseRouting();
                                app.UseEndpoints(routeBuilder => routeBuilder.MapControllers());

                                app.UseSwagger();
                                app.UseSwaggerUI(
                                    swaggerUiOptions =>
                                    {
                                        swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Vektonn API V1");
                                        swaggerUiOptions.RoutePrefix = string.Empty;
                                        swaggerUiOptions.DocumentTitle = "Vektonn API";
                                        swaggerUiOptions.DefaultModelsExpandDepth(2);
                                    });
                            }));

            hostBuilder.Build().Run();
        }

        private static void LogMemoryUsage(string message)
        {
            CollectGarbageWithLohCompaction();

            var currentProcess = Process.GetCurrentProcess();
            var privateMb = currentProcess.PrivateMemorySize64 / (1024 * 1024);
            var workingSetMb = currentProcess.WorkingSet64 / (1024 * 1024);
            Console.Out.WriteLine($"{message} privateMb: {privateMb}, workingSetMb: {workingSetMb}");
        }

        private static void CollectGarbageWithLohCompaction()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(generation: 2, GCCollectionMode.Forced, blocking: true, compacting: true);
        }
    }
}
