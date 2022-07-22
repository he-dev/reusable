using System.Diagnostics;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

public class DebugChannel : Channel<DebugChannel>
{
    public string Template { get; set; } = "[{Timestamp:HH:mm:ss:fff}] [{Logger}] {Message}";

    protected override void Log(ILogEntry entry)
    {
        Debug.WriteLine(Template.Format(entry));
        
        Next?.Invoke(entry);
    }
}