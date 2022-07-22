using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Channels;

public class NLogChannel : Channel<NLogChannel>
{
    private readonly ConcurrentDictionary<string, NLog.Logger> _cache = new(SoftString.Comparer);

    // private static readonly Dictionary<string, NLog.LogLevel> LogLevels = new(SoftString.Comparer)
    // {
    //     ["Trace"] = NLog.LogLevel.Trace,
    //     ["Debug"] = NLog.LogLevel.Debug,
    //     ["Information"] = NLog.LogLevel.Info,
    //     ["Warning"] = NLog.LogLevel.Warn,
    //     ["Error"] = NLog.LogLevel.Error,
    //     ["Fatal"] = NLog.LogLevel.Fatal,
    // };

    protected override void Log(ILogEntry entry)
    {
        var loggerName = entry.GetValueOrDefault("Logger", "Unknown");
        _cache
            .GetOrAdd(loggerName, NLog.LogManager.GetLogger)
            .Log(CreateLogEventInfo(entry));
        
        Next?.Invoke(entry);
    }

    private static NLog.LogEventInfo CreateLogEventInfo(ILogEntry entry)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = NLog.LogLevel.Off,
            LoggerName = entry.GetValueOrDefault("Logger", "Unknown"),
            Message = entry.GetValueOrDefault("Message", default(string)),
            Exception = entry.GetValueOrDefault("Exception", default(Exception)),
            TimeStamp = entry.GetValueOrDefault("Timestamp", DateTime.UtcNow),
        };

        foreach (var property in entry.WhereTag<IRegularProperty>())
        {
            logEventInfo.Properties.Add(property.Name, property.Value);
        }

        return logEventInfo;
    }

    // private static NLog.LogLevel MapLogLevel(ILogEntry entry)
    // {
    //     if (entry[LogProperty.Names.Level()].Value is LogLevel level && LogLevels.TryGetValue(level.ToString(), out var mapped))
    //     {
    //         return mapped;
    //     }
    //
    //     return NLog.LogLevel.Info;
    // }
}