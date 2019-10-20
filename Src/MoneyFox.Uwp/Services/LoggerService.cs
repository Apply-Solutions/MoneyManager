﻿using System.IO;
using Windows.Storage;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MoneyFox.Uwp.Services
{
    public static class LoggerService
    {
        public static void Initialize()
        {
            var config = new LoggingConfiguration();

            var logfile = new FileTarget("logfile")
            {
                FileName = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "moneyfox.log"),
                AutoFlush = true,
                ArchiveEvery = FileArchivePeriod.Month
            };
            var debugTarget = new DebugTarget("console");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, debugTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }
    }
}
