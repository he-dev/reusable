using System;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node adds support for <c>Logger.Log(log => {})</c> overload.
/// </summary>
public class EntryAction : LoggerNode
{
    public override void Invoke(ILogEntry entry)
    {
        if (entry[nameof(Action)].Value is Action<ILogEntry> action)
        {
            action(entry);
            entry.Push(new LogProperty.Deleted(nameof(Action)));
        }
            
        InvokeNext(entry);
    }
}