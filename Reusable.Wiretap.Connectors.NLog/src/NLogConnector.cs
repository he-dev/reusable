﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Connectors
{
    public class NLogConnector : IConnector
    {
        private readonly ConcurrentDictionary<SoftString, NLog.Logger> _cache = new ConcurrentDictionary<SoftString, NLog.Logger>();

        private static readonly IDictionary<LogLevel, NLog.LogLevel> LogLevels = new Dictionary<LogLevel, NLog.LogLevel>
        {
            [LogLevel.Trace] = NLog.LogLevel.Trace,
            [LogLevel.Debug] = NLog.LogLevel.Debug,
            [LogLevel.Information] = NLog.LogLevel.Info,
            [LogLevel.Warning] = NLog.LogLevel.Warn,
            [LogLevel.Error] = NLog.LogLevel.Error,
            [LogLevel.Fatal] = NLog.LogLevel.Fatal,
        };

        public void Log(ILogEntry logEntry)
        {
            var loggerName = logEntry.GetValueOrDefault(Names.Properties.Logger, "Undefined");
            GetLogger(loggerName).Log(CreateLogEventInfo(logEntry));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(ILogEntry logEntry)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevels[logEntry.GetValueOrDefault(Names.Properties.Level, LogLevel.Information)],
                LoggerName = logEntry.GetValueOrDefault(Names.Properties.Logger, "Undefined"),
                Message = logEntry.GetValueOrDefault(Names.Properties.Message, default(string)),
                Exception = logEntry.GetValueOrDefault(Names.Properties.Exception, default(Exception)),
                TimeStamp = logEntry.GetValueOrDefault(Names.Properties.Timestamp, DateTime.UtcNow),
            };

            foreach (var item in logEntry.Where(LogProperty.CanLog.With<NLogConnector>()))
            {
                logEventInfo.Properties.Add(item.Name, item.Value);
            }

            return logEventInfo;
        }

        private NLog.ILogger GetLogger(string name)
        {
            return _cache.GetOrAdd(name!, n => NLog.LogManager.GetLogger(name));
        }
    }
}