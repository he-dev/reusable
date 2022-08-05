using System.Diagnostics;
using Reusable.Marbles.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

public class DebugChannel : Channel<DebugChannel>
{
    public DebugChannel(string? name = default) : base(name) { }
    
    public string Template { get; set; } = Telemetry.Template;

    protected override void Log(ILogEntry entry)
    {
        Debug.WriteLine(Template.Format(entry));
        
        Next?.Invoke(entry);
    }

}