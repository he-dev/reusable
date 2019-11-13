using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog
{
    public class NLogRx : ILogRx
    {
        private readonly ConcurrentDictionary<SoftString, NLog.Logger> _cache = new ConcurrentDictionary<SoftString, NLog.Logger>();

        private static readonly IDictionary<Option<LogLevel>, NLog.LogLevel> LogLevels = new Dictionary<Option<LogLevel>, NLog.LogLevel>
        {
            [LogLevel.Trace] = NLog.LogLevel.Trace,
            [LogLevel.Debug] = NLog.LogLevel.Debug,
            [LogLevel.Information] = NLog.LogLevel.Info,
            [LogLevel.Warning] = NLog.LogLevel.Warn,
            [LogLevel.Error] = NLog.LogLevel.Error,
            [LogLevel.Fatal] = NLog.LogLevel.Fatal,
        };

        public void Log(LogEntry logEntry)
        {
            var loggerName = logEntry.GetPropertyOrDefault<Log>(LogEntry.Names.Logger).ValueOrDefault<string>();
            GetLogger(loggerName).Log(CreateLogEventInfo(logEntry));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(LogEntry logEntry)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevels[logEntry.GetPropertyOrDefault<Log>(LogEntry.Names.Level).ValueOrDefault<Option<LogLevel>>()],
                LoggerName = logEntry.GetPropertyOrDefault<Log>(LogEntry.Names.Logger).ValueOrDefault<string>(),
                Message = logEntry.GetPropertyOrDefault<Log>(LogEntry.Names.Message).ValueOrDefault<string>(),
                Exception = logEntry.GetPropertyOrDefault<Log>(LogEntry.Names.Exception).ValueOrDefault<Exception>(),
                TimeStamp = logEntry.GetPropertyOrDefault<Log>(LogEntry.Names.Timestamp).ValueOrDefault<DateTime>(),
            };

            foreach (var item in logEntry.Action<Log>())
            {
                logEventInfo.Properties.Add(item.Name.ToString(), item.Property.Value);
            }

            return logEventInfo;
        }

        private NLog.ILogger GetLogger(string name)
        {
            return _cache.GetOrAdd(name, n => NLog.LogManager.GetLogger(name));
        }

        public static NLogRx Create() => new NLogRx();
    }
}