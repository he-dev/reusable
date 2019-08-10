using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog
{
    public class NLogRx : ILogRx
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

        public void Log(LogEntry logEntry)
        {
            var loggerName = logEntry.GetItemOrDefault<string>(LogEntry.Names.Logger, default);
            GetLogger(loggerName).Log(CreateLogEventInfo(logEntry));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(LogEntry logEntry)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevels[logEntry.GetItemOrDefault<LogLevel>(LogEntry.Names.Level, default)],
                LoggerName = logEntry.GetItemOrDefault<string>(LogEntry.Names.Logger, default),
                Message = logEntry.GetItemOrDefault<string>(LogEntry.Names.Message, default),
                Exception = logEntry.GetItemOrDefault<Exception>(LogEntry.Names.Exception, default),
                TimeStamp = logEntry.GetItemOrDefault<DateTime>(LogEntry.Names.Timestamp, default),
            };

            foreach (var item in logEntry.Where(x => x.Key.Tag.Equals(LogEntry.DefaultItemTag)))
            {
                logEventInfo.Properties.Add(item.Key.Name.ToString(), item.Value);
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