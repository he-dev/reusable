using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Middleware;

/// <summary>
/// This node filters log-entries and short-circuits the pipeline.
/// </summary>
public class FilterEntries : LoggerMiddleware
{
    public FilterEntries(Func<ILogEntry, bool> canLog) => CanLog = canLog;

    private Func<ILogEntry, bool> CanLog { get; }

    public override void Invoke(ILogEntry entry)
    {
        if (CanLog(entry))
        {
            Next?.Invoke(entry);
        }
    }
}