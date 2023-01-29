using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Channels;

public class LogToNLog : Channel
{
    private readonly ConcurrentDictionary<string, NLog.Logger> _cache = new(SoftString.Comparer);

    protected override void InvokeThis(LogEntry entry)
    {
        var loggerName = entry.GetValueOrDefault(AttachScope.Key, "Default");
        _cache
            .GetOrAdd(loggerName, NLog.LogManager.GetLogger)
            .Log(CreateLogEventInfo(entry));
    }

    private static NLog.LogEventInfo CreateLogEventInfo(LogEntry entry)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = entry.GetValueOrDefault("Level", "i") switch
            {
                "i" => NLog.LogLevel.Info,
                "d" => NLog.LogLevel.Debug,
                "w" => NLog.LogLevel.Warn,
                "e" => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Info
            },
            LoggerName = (string)entry.GetValueOrDefault(AttachScope.Key, "Default"),
            //Message = entry.GetValueOrDefault("Message", default(string)),
            Exception = entry.GetValueOrDefault("Exception", default(Exception)),
            TimeStamp = entry.GetValueOrDefault(AttachTimestamp.Key, DateTime.UtcNow),
        };

        foreach (var item in entry)
        {
            logEventInfo.Properties.Add(item.Key, item.Value);
        }

        return logEventInfo;
    }
}