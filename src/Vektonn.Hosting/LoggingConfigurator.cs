using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.Context;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;

namespace Vektonn.Hosting
{
    public static class LoggingConfigurator
    {
        public static ILog SetupLocalLog(string? hostingEnvironment = null)
        {
            SetupUnhandledExceptionLogging();

            var logs = new List<ILog>();
            if (hostingEnvironment == Constants.DevHostingEnvironment)
            {
                logs.Add(
                    new ConsoleLog(
                        new ConsoleLogSettings
                        {
                            ColorsEnabled = true,
                        }));
            }

            var appDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory ?? throw new InvalidOperationException("AppDomain.CurrentDomain.BaseDirectory is not set");
            logs.Add(
                new FileLog(
                    new FileLogSettings
                    {
                        Encoding = Encoding.UTF8,
                        FileOpenMode = FileOpenMode.Rewrite,
                        RollingStrategy = new RollingStrategyOptions {Type = RollingStrategyType.None},
                        FilePath = Path.Combine(appDomainBaseDirectory, "logs", $"{DateTime.Now:yyyy-MM-dd.HH-mm-ss}.log"),
                    }));

            var localLog = new CompositeLog(logs.ToArray())
                .ForContext("Local")
                .WithAllFlowingContextProperties();

            return localLog;
        }

        private static void SetupUnhandledExceptionLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "Unhandled exception in current AppDomain");
        }

        private static void LogUnhandledException(Exception? exception, string logMessage)
        {
            Console.WriteLine(exception);
            LogProvider.Get().Fatal(exception, logMessage);
        }
    }
}
