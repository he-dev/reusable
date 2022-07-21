using System;
using System.Collections.Generic;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes.Scopeable;

/// <summary>
/// This node temporarily stores log-entries. You need to use Flush to log and empty the buffer. This node is disabled by default. 
/// </summary>
public class ScopeBuffer : LoggerNode
{
    private Queue<ILogEntry> Entries { get; } = new();

    public override void Invoke(ILogEntry entry)
    {
        var enabled = entry[LogProperty.Names.LoggerScope()].Value is ILoggerScope scope && scope.Items.TryGetValue(nameof(ScopeBuffer), out var flag) && flag is true;

        if (enabled)
        {
            switch (entry[nameof(ScopeBuffer)].Value)
            {
                case Mode.Force:
                    Next?.Invoke(entry);
                    break;
                case Mode.Defer:
                    Entries.Enqueue(entry);
                    break;
                default:
                    throw new InvalidOperationException($"With enabled {nameof(ScopeBuffer)} you must use either {Mode.Force} or {Mode.Defer} when logging.");
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

    public override void Dispose()
    {
        Entries.Clear();
        base.Dispose();
    }

    public enum Mode
    {
        None,
        Force,
        Defer
    }
}