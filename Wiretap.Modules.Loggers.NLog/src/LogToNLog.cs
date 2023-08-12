using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules.Loggers;

public class LogToNLog : ILog
{
    public string Name { get; set; } = nameof(Wiretap);

    private NLog.Logger? Logger { get; set; }

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        (Logger ??= NLog.LogManager.GetLogger(Name)).Log(CreateLogEventInfo(activity, entry));
        next(activity, entry);
    }

    private static NLog.LogEventInfo CreateLogEventInfo(IActivity activity, LogEntry entry)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = entry.GetItemOrDefault(SetLevel.Key, "info") switch
            {
                "info" => NLog.LogLevel.Info,
                "debug" => NLog.LogLevel.Debug,
                "warning" => NLog.LogLevel.Warn,
                "error" => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Info
            },
            LoggerName = activity.Name,
            Message = entry.GetItemOrDefault("Message", default(string)),
            Exception = entry.GetItemOrDefault(LogEntry.PropertyNames.Attachment, default(Exception)),
            TimeStamp = entry.Timestamp() ?? DateTime.UtcNow,
        };

        foreach (var item in entry)
        {
            logEventInfo.Properties.Add(item.Key, item.Value);
        }

        return logEventInfo;
    }
}