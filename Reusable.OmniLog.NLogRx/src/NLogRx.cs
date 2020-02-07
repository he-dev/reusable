using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

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
            var loggerName = logEntry.GetProperty(LogEntry.Names.Logger, m => m.ProcessWith<EchoNode>()).ValueOrDefault<string>();
            GetLogger(loggerName).Log(CreateLogEventInfo(logEntry));
        }

        private static NLog.LogEventInfo CreateLogEventInfo(LogEntry logEntry)
        {
            var logEventInfo = new NLog.LogEventInfo
            {
                Level = LogLevels[logEntry.GetProperty(LogEntry.Names.Level, m => m.ProcessWith<EchoNode>()).ValueOrDefault<Option<LogLevel>>()],
                LoggerName = logEntry.GetProperty(LogEntry.Names.Logger, m => m.ProcessWith<EchoNode>()).ValueOrDefault<string>(),
                Message = logEntry.GetProperty(LogEntry.Names.Message, m => m.ProcessWith<EchoNode>()).ValueOrDefault<string>(),
                Exception = logEntry.GetProperty(LogEntry.Names.Exception, m => m.ProcessWith<EchoNode>()).ValueOrDefault<Exception>(),
                TimeStamp = logEntry.GetProperty(LogEntry.Names.Timestamp, m => m.ProcessWith<EchoNode>()).ValueOrDefault<DateTime>(),
            };

            foreach (var item in logEntry.Properties(m => m.ProcessWith<EchoNode>().LogWith<NLogRx>()))
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