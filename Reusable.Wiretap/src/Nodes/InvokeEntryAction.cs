using System;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node adds support for <c>Logger.Log(log => {})</c> overload.
/// </summary>
public class InvokeEntryAction : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        //if (entry.TryGetProperty(nameof(CallableProperty.EntryAction), out var property) && property?.Value is Action<ILogEntry> action)
        if (entry.TryGetProperty<CallableProperty.EntryAction, Action<ILogEntry>>(out var action))
        {
            entry.Also(action);
        }
            
        InvokeNext(entry);
    }
}