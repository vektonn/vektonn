using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                                    .AddJsonOptions(options => HttpJson.Configure(options.JsonSerializerOptions));

                                services.AddSwaggerGen();
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
                                        swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "SpaceHosting API V1");
                                        swaggerUiOptions.RoutePrefix = string.Empty;
                                    });
                            }));

            hostBuilder.Build().Run();
        }
    }
}
