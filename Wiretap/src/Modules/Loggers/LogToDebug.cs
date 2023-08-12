using System.Diagnostics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules.Loggers;

public class LogToDebug : ILog
{
    public string Template { get; set; } = "{timestamp:HH:mm:ss:fff} | {activity} | {trace} | {elapsed} | {message} | {details} | {attachment}";

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        Debug.WriteLine(Template.Format(entry.TryGetValue));
        next(activity, entry);
    }
}