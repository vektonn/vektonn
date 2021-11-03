using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vektonn.ApiContracts.Json;
using Vektonn.Hosting.Configuration;
using Vektonn.SharedImpl.Configuration;
using Vostok.Applications.AspNetCore;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vektonn.Hosting
{
    public abstract class VektonnApplicationBase : VostokAspNetCoreApplication, IVektonnApplication
    {
        public abstract string ApplicationName { get; }

        public override string ToString()
        {
            return $"ApplicationName: {ApplicationName}";
        }

        public sealed override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment hostingEnvironment)
        {
            builder.SetupWebHost(webHostBuilder => SetupWebHostBuilder(webHostBuilder, hostingEnvironment));
        }

        protected abstract void ConfigureServices(IServiceCollection services);

        protected virtual void ConfigureApp(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(routeBuilder => routeBuilder.MapControllers());
        }

        private void SetupWebHostBuilder(IWebHostBuilder webHostBuilder, IVostokHostingEnvironment hostingEnvironment)
        {
            webHostBuilder.ConfigureServices(
                services =>
                {
                    services.AddSingleton(hostingEnvironment.Log);
                    services.AddSingleton(hostingEnvironment.Tracer);
                    services.AddSingleton(hostingEnvironment.Metrics);

                    services.AddSingleton<IShutdownTokenProvider, ShutdownTokenProvider>();

                    services.AddSingleton<KafkaConfigurationProvider>();

                    services.AddSingleton<IIndexMetaProvider>(BuildIndexMetaProvider(hostingEnvironment.Log));

                    services
                        .AddControllers()
                        .AddApplicationPart(Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Assembly.GetEntryAssembly() is not defined"))
                        .AddJsonOptions(jsonOptions => HttpJson.Configure(jsonOptions.JsonSerializerOptions));

                    ConfigureServices(services);
                });

            webHostBuilder.Configure(ConfigureApp);
        }

        private static IndexMetaProvider BuildIndexMetaProvider(ILog log)
        {
            var configBaseDirectory = FileSystemHelpers.PatchDirectoryName(EnvironmentVariables.Get("VEKTONN_CONFIG_BASE_DIRECTORY"));
            if (!Directory.Exists(configBaseDirectory))
                throw new InvalidOperationException($"configBaseDirectory does not exist: {configBaseDirectory}");

            log.Info($"Using configBaseDirectory: {configBaseDirectory}");

            return new IndexMetaProvider(configBaseDirectory);
        }
    }
}
