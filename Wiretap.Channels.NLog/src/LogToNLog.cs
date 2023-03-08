using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Channels;

public class LogToNLog : Channel
{
    public string Name { get; set; } = nameof(Wiretap);

    private NLog.Logger? Logger { get; set; }

    protected override void InvokeThis(LogEntry entry)
    {
        (Logger ??= NLog.LogManager.GetLogger(Name)).Log(CreateLogEventInfo(entry));
    }

    private static NLog.LogEventInfo CreateLogEventInfo(LogEntry entry)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = entry.GetValueOrDefault(AttachLevel.Key, "info") switch
            {
                "info" => NLog.LogLevel.Info,
                "debug" => NLog.LogLevel.Debug,
                "warning" => NLog.LogLevel.Warn,
                "error" => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Info
            },
            LoggerName = entry.GetValueOrDefault(AttachScope.Key, "Default"),
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