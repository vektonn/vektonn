using System;
using System.Net;
using System.Runtime;
using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;

namespace Vektonn.Hosting
{
    public class VektonnApplicationHost<TApplication> : IVektonnApplicationHost
        where TApplication : IVektonnApplication, new()
    {
        private readonly ILog localLog;
        private readonly VostokHost vostokHost;

        public VektonnApplicationHost(VektonnApplicationHostSettings hostSettings)
            : this(hostSettings, new TApplication())
        {
        }

        public VektonnApplicationHost(VektonnApplicationHostSettings hostSettings, TApplication application)
        {
            localLog = LoggingConfigurator.SetupLocalLog(application.ApplicationName, hostSettings.HostingEnvironment);

            var vostokHostSettings = new VostokHostSettings(
                application,
                hostingEnvironmentBuilder => SetupHosting(hostSettings, hostingEnvironmentBuilder, application))
            {
                LogApplicationConfiguration = true
            };

            vostokHost = new VostokHost(vostokHostSettings).WithConsoleCancellation();
        }

        public Task StartAsync()
        {
            return vostokHost.StartAsync(stateToAwait: VostokApplicationState.Running);
        }

        public Task StopAsync()
        {
            return vostokHost.StopAsync(ensureSuccess: true);
        }

        public Task RunToCompletionAsync()
        {
            return vostokHost.StartAsync(stateToAwait: VostokApplicationState.Exited);
        }

        private void SetupHosting(VektonnApplicationHostSettings hostSettings, IVostokHostingEnvironmentBuilder hostingEnvironmentBuilder, IVektonnApplication application)
        {
            localLog.Info(
                "Setting up VostokHost. " +
                $"HostSettings: {hostSettings}, " +
                $"Application: {application}, " +
                $"GCSettings.IsServerGC: {GCSettings.IsServerGC}, " +
                $"GCSettings.LatencyMode: {GCSettings.LatencyMode}");

            hostingEnvironmentBuilder
                .SetupApplicationIdentity(
                    identityBuilder =>
                    {
                        identityBuilder.SetProject(Constants.VostokProject);
                        identityBuilder.SetApplication(application.ApplicationName);
                        identityBuilder.SetEnvironment(hostSettings.HostingEnvironment.ToLower());
                        identityBuilder.SetInstance(Dns.GetHostName().ToLower());
                    })
                .SetupLog(
                    logBuilder =>
                    {
                        logBuilder
                            .SetupFileLog(fileLogBuilder => fileLogBuilder.Disable())
                            .SetupConsoleLog(consoleLogBuilder => consoleLogBuilder.Disable())
                            .SetupHerculesLog(herculesLogBuilder => herculesLogBuilder.Disable())
                            .AddLog("LocalLog", localLog)
                            .AddRule(new LogConfigurationRule {Source = "Microsoft.AspNetCore", MinimumLevel = LogLevel.Warn});
                    })
                .SetupSystemMetrics(
                    metricsSettings =>
                    {
                        metricsSettings.EnableHostMetricsLogging = true;
                        metricsSettings.EnableHostMetricsReporting = true;
                        metricsSettings.HostMetricsLoggingPeriod = TimeSpan.FromSeconds(30);
                        metricsSettings.ProcessMetricsLoggingPeriod = TimeSpan.FromSeconds(30);
                    })
                .SetPort(hostSettings.Port)
                .SetupShutdownTimeout(hostSettings.ShutdownTimeout)
                .DisableClusterConfig()
                .DisableHercules()
                .DisableZooKeeper()
                .DisableServiceBeacon();
        }
    }
}
