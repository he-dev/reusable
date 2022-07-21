using System.Diagnostics;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

public class DebugChannel : Channel
{
    public string Template { get; set; } = "[{Timestamp:HH:mm:ss:fff}] [{Logger}] {Message}";

    public override void Invoke(ILogEntry entry)
    {
        Debug.WriteLine(Template.Format(entry));
        
        Next?.Invoke(entry);
    }
}