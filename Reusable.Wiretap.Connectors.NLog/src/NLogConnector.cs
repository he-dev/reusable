using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Connectors;

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
        var loggerName = logEntry.GetValueOrDefault(LogProperty.Names.Logger, "Undefined");
        GetLogger(loggerName).Log(CreateLogEventInfo(logEntry));
    }

    private static NLog.LogEventInfo CreateLogEventInfo(ILogEntry logEntry)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = LogLevels[logEntry.GetValueOrDefault(LogProperty.Names.Level, LogLevel.Information)],
            LoggerName = logEntry.GetValueOrDefault(LogProperty.Names.Logger, "Undefined"),
            Message = logEntry.GetValueOrDefault(LogProperty.Names.Message, default(string)),
            Exception = logEntry.GetValueOrDefault(LogProperty.Names.Exception, default(Exception)),
            TimeStamp = logEntry.GetValueOrDefault(LogProperty.Names.Timestamp, DateTime.UtcNow),
        };

        foreach (var item in logEntry.Where(p => p is LoggableProperty))
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