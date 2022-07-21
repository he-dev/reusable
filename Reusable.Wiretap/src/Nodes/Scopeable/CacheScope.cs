using System.Collections;
using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Nodes.Scopeable;

/// <summary>
/// This node caches logs in memory. It is disabled by default.
/// </summary>
public class CacheScope : LoggerNode, IEnumerable<ILogEntry>
{
    public int Capacity { get; set; } = 10_000;

    private Queue<ILogEntry> Entries { get; } = new();

    public override void Invoke(ILogEntry entry)
    {
        lock (Entries)
        {
            Entries.Enqueue(entry);

            if (Entries.Count > Capacity)
            {
                Entries.Dequeue();
            }
        }

        Next?.Invoke(entry);
    }

    public IEnumerator<ILogEntry> GetEnumerator() => Entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Entries).GetEnumerator();

    public override void Dispose()
    {
        Entries.Clear();
        base.Dispose();
    }
}