using System;
using Reusable.Extensions;
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