using System;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Nodes;

public class Intercept : LoggerNode
{
    public Intercept(Action<ILogEntry> action) => Action = action;

    private Action<ILogEntry>? Action { get; } 
    
    public override void Invoke(ILogEntry entry)
    {
        entry.Also(Action);
        Next?.Invoke(entry);
    }
}