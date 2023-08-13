using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules.Loggers;

public class LogToNLog : ILog
{
    public string Name { get; set; } = nameof(Wiretap);
    
    public IDictionary<string, NLog.LogLevel> Levels { get; set; } = new Dictionary<string, NLog.LogLevel>(SoftString.Comparer)
    {
        { Strings.Traces.Begin, NLog.LogLevel.Info },
        { Strings.Traces.Info, NLog.LogLevel.Debug },
        { Strings.Traces.Noop, NLog.LogLevel.Debug },
        { Strings.Traces.Break, NLog.LogLevel.Debug },
        { Strings.Traces.End, NLog.LogLevel.Info },
        { Strings.Traces.Error, NLog.LogLevel.Error },
    };

    private NLog.Logger? Logger { get; set; }

    public void Invoke(TraceContext context, LogFunc next)
    {
        (Logger ??= NLog.LogManager.GetLogger(Name)).Log(CreateLogEventInfo(context));
        next(context);
    }

    private NLog.LogEventInfo CreateLogEventInfo(TraceContext context)
    {
        var logEventInfo = new NLog.LogEventInfo
        {
            Level = Levels.TryGetValue(context.Entry.GetItem<string>(Strings.Items.Trace)!, out var trace) ? trace : NLog.LogLevel.Info,
            LoggerName = context.Activity.Name,
            Message = context.Entry.GetItem<string>(Strings.Items.Message),
            Exception = context.Entry.GetItem<Exception>(Strings.Items.Attachment),
            TimeStamp = context.Entry.Timestamp() ?? DateTime.UtcNow,
        };

        foreach (var item in context.Entry)
        {
            logEventInfo.Properties.Add(item.Key, item.Value);
        }

        return logEventInfo;
    }
}