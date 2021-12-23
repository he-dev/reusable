using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Connectors;

public class NLogConnector : IConnector
{
    private readonly ConcurrentDictionary<SoftString, NLog.Logger> _cache = new();

    private static readonly Dictionary<string, NLog.LogLevel> LogLevels = new()
    {
        ["Trace"] = NLog.LogLevel.Trace,
        ["Debug"] = NLog.LogLevel.Debug,
        ["Information"] = NLog.LogLevel.Info,
        ["Warning"] = NLog.LogLevel.Warn,
        ["Error"] = NLog.LogLevel.Error,
        ["Fatal"] = NLog.LogLevel.Fatal,
    };

    public void Log(ILogEntry logEntry)
    {
        var loggerName = logEntry.GetValueOrDefault("Logger", "Unknown");
        _cache
            .GetOrAdd(loggerName, n => NLog.LogManager.GetLogger(loggerName))
            .Log(CreateLogEventInfo(logEntry));
    }

    private static NLog.LogEventInfo CreateLogEventInfo(ILogEntry logEntry)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = LogLevels[logEntry.TryGetProperty("Level", out var level) ? level.Value.ToString()! : "Information"],
            LoggerName = logEntry.GetValueOrDefault("Logger", "Unknown"),
            Message = logEntry.GetValueOrDefault("Message", default(string)),
            Exception = logEntry.GetValueOrDefault("Exception", default(Exception)),
            TimeStamp = logEntry.GetValueOrDefault("Timestamp", DateTime.UtcNow),
        };

        foreach (var property in logEntry.Where<ILoggableProperty>())
        {
            logEventInfo.Properties.Add(property.Name, property.Value);
        }

        return logEventInfo;
    }
}