using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node filters log-entries and short-circuits the pipeline.
/// </summary>
public class FilterEntries : LoggerNode
{
    public FilterEntries(Func<ILogEntry, bool> canLog) => CanLog = canLog;
    
    public Func<ILogEntry, bool> CanLog { get; }

    public override void Invoke(ILogEntry entry)
    {
        if (CanLog(entry))
        {
            Next?.Invoke(entry);
        }
    }
}