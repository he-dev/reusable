using System;
using System.Collections.Generic;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Middleware;

/// <summary>
/// This node temporarily stores log-entries. You need to use Flush to log and empty the buffer. This node is disabled by default. 
/// </summary>
public class UnitOfWorkBuffer : LoggerMiddleware
{
    public bool Enabled { get; set; }
    
    private Queue<ILogEntry> Entries { get; } = new();

    public override void Invoke(ILogEntry entry)
    {
        if (Enabled)
        {
            switch (entry[nameof(UnitOfWorkBuffer)].Value)
            {
                case Mode.Force:
                    Next?.Invoke(entry);
                    break;
                case Mode.Defer:
                    Entries.Enqueue(entry);
                    break;
                default:
                    throw new InvalidOperationException($"With enabled {nameof(UnitOfWorkBuffer)} you must use either {Mode.Force} or {Mode.Defer} when logging.");
            }
        }
        else
        {
            Next?.Invoke(entry);
        }
    }

    public int Count => Entries.Count;

    public void Flush()
    {
        foreach (var entry in Entries.Consume())
        {
            Next?.Invoke(entry);
        }
    }

    public void Clear()
    {
        Entries.Clear();
    }
    
    public enum Mode
    {
        None,
        Force,
        Defer
    }
}