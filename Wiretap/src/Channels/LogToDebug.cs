using System.Diagnostics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Channels;

public class LogToDebug : Channel
{
    public string Template { get; set; } = "{timestamp:yyyy-MM-dd HH:mm:ss:fff} | {parentId}:{id} | {status} | {elapsed}s | {details} | {attachment}";

    protected override void InvokeThis(LogEntry entry)
    {
        Debug.WriteLine(Template.Format(entry));
    }
}