using System;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Nodes;

public class Debug : LoggerNode
{
    public Action<ILogEntry>? Action { get; set; } 
    
    public override void Invoke(ILogEntry entry)
    {
        entry.Also(Action);
        InvokeNext(entry);
    }
}