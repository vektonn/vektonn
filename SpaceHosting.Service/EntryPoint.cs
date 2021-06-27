using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SpaceHosting.Json;
using SpaceHosting.Service.IndexStore;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace SpaceHosting.Service
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine($"EnvironmentVariables:{EnvironmentVariables.GetAll(name => name.StartsWith("SH_")).ToPrettyJson()}");

            var hostBuilder = Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    builder => builder
                        .ConfigureServices(
                            services =>
                            {
                                services.AddSingleton<ILog>(new SynchronousConsoleLog());
                                services.AddSingleton<IndexStoreBuilder>();
                                services.AddSingleton(s => s.GetRequiredService<IndexStoreBuilder>().BuildIndexStore());

                                services
                                    .AddControllers()
                                    .AddJsonOptions(jsonOptions => HttpJson.Configure(jsonOptions.JsonSerializerOptions));

                                services.AddSwaggerGen(
                                    swaggerGenOptions =>
                                    {
                                        swaggerGenOptions.UseOneOfForPolymorphism();

                                        swaggerGenOptions.IgnoreObsoleteActions();
                                        swaggerGenOptions.IgnoreObsoleteProperties();

                                        swaggerGenOptions.SwaggerDoc(
                                            "v1",
                                            new OpenApiInfo
                                            {
                                                Title = "SpaceHosting API V1",
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
                                        swaggerUiOptions.DocumentTitle = "SpaceHosting API";
                                        swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "SpaceHosting API V1");
                                        swaggerUiOptions.RoutePrefix = string.Empty;
                                    });
                            }));

            hostBuilder.Build().Run();
        }
    }
}
