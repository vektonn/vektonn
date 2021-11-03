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
        public static ILog SetupLocalLog(string applicationName, string? hostingEnvironment = null)
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

            logs.Add(
                new FileLog(
                    new FileLogSettings
                    {
                        Encoding = Encoding.UTF8,
                        FileOpenMode = FileOpenMode.Append,
                        FilePath = Path.Combine(
                            FileSystemHelpers.PatchDirectoryName("logs"),
                            $"{applicationName}.{{RollingSuffix}}.{DateTime.Now:HH-mm-ss}.log"),
                        RollingStrategy = new RollingStrategyOptions
                        {
                            Type = RollingStrategyType.ByTime,
                            Period = RollingPeriod.Day,
                            MaxFiles = 30
                        }
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
