using System;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This node adds support for <c>Logger.Log(log => ..)</c> overload.
    /// </summary>
    public class InvokeEntryAction : LoggerNode
    {
        public override void Invoke(ILogEntry entry)
        {
            if (entry.TryGetProperty(nameof(CallableProperty.EntryAction), out var property) && property?.Value is Action<ILogEntry> action)
            {
                entry.Also(action);
            }
            
            InvokeNext(entry);
        }
    }
}