using System.Diagnostics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Modules.Loggers;

public class LogToDebug : ILog
{
    public string Template { get; set; } = "{timestamp:HH:mm:ss:fff} | {activity} | {trace} | {elapsed} | {message} | {details} | {attachment}";

    public void Invoke(TraceContext context, LogAction next)
    {
        Debug.WriteLine(Template.Format(context.Entry.TryGetValue));
        next(context);
    }
}