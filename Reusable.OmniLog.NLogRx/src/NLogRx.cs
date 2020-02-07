using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

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

        public void Log(ILogEntry logEntry)
        {
            var loggerName = logEntry[LogProperty.Names.Logger]?.Value as string;
            GetLogger(loggerName).Log(CreateLogEventInfo(logEntry));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(ILogEntry logEntry)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevels[logEntry[LogProperty.Names.Level]?.Value as Option<LogLevel> ?? LogLevel.Information],
                LoggerName = logEntry[LogProperty.Names.Logger]?.Value as string,
                Message = logEntry[LogProperty.Names.Message]?.Value as string,
                Exception = logEntry[LogProperty.Names.Exception]?.Value as Exception,
                TimeStamp = logEntry[LogProperty.Names.Timestamp]?.Value is DateTime dt ? dt : DateTime.UtcNow,
            };

            foreach (var item in logEntry.Where(LogProperty.CanLog.With<NLogRx>()))
            {
                logEventInfo.Properties.Add(item.Name.ToString(), item.Value);
            }

            return logEventInfo;
        }

        private NLog.ILogger GetLogger(string name)
        {
            return _cache.GetOrAdd(name!, n => NLog.LogManager.GetLogger(name));
        }

        public static NLogRx Create() => new NLogRx();
    }
}