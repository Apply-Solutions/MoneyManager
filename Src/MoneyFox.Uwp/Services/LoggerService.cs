﻿using MoneyFox.Application.Common.Constants;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.IO;
using Xamarin.Essentials;

#if !DEBUG
using PCLAppConfig;
#endif

#nullable enable
namespace MoneyFox.Uwp.Services
{
    public static class LoggerService
    {
        public static void Initialize()
        {
            var config = new LoggingConfiguration();

            // Configure file
            var logfile = new FileTarget("logfile")
            {
                FileName = Path.Combine(FileSystem.CacheDirectory, AppConstants.LogFileName),
                AutoFlush = true,
                ArchiveEvery = FileArchivePeriod.Month
            };

            // Configure console
            var debugTarget = new DebugTarget("console");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, debugTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }
    }
}
