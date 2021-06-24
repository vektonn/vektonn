using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaceHosting.Json;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace SpaceHosting.Service
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
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
                            })
                        .Configure(
                            app =>
                            {
                                app.UseRouting();
                                app.UseEndpoints(routeBuilder => routeBuilder.MapControllers());
                            }));

            hostBuilder.Build().Run();
        }
    }
}
