using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

[PublicAPI]
public class MemoryChannel : Channel<MemoryChannel>
{
    public const int DefaultCapacity = 1_000;

    public Queue<ILogEntry> Entries { get; } = new();

    public MemoryChannel(int capacity = DefaultCapacity) => Capacity = capacity;

    public int Capacity { get; }

    protected override void Log(ILogEntry entry)
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
}