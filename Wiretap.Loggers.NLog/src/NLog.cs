using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Reusable.Wiretap.Loggers;

public class NLog : ILogger
{
    public IDictionary<Severity, LogLevel> SeverityMapping { get; set; } = new Dictionary<Severity, LogLevel>()
    {
        { Severity.High, LogLevel.Error },
        { Severity.Normal, LogLevel.Info },
        { Severity.Low, LogLevel.Debug },
    };

    private global::NLog.Logger? Logger { get; set; }

    public void Log(IEnumerable<KeyValuePair<string, object>> properties)
    {
        var logEventInfo = new LogEventInfo
        {
            Level = LogLevel.Info
        };

        foreach (var property in properties)
        {
            switch (property.Value)
            {
                case TraceProperty t:
                    logEventInfo.Message = t.Message;
                    logEventInfo.LoggerName = t.Name;
                    break;

                case Severity s:
                    logEventInfo.Level = SeverityMapping[s];
                    continue;
            }

            logEventInfo.Properties.Add(property.Key, property.Value);
        }

        (Logger ??= LogManager.GetLogger("Wiretap")).Log(logEventInfo);
    }
}