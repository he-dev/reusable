using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Channels;

[PublicAPI]
public class LogToMemory : Channel
{
    public const int DefaultCapacity = 1_000_000;

    public Queue<LogEntry> Entries { get; } = new();

    public int Capacity { get; set; } = DefaultCapacity;
    
    protected override void InvokeThis(LogEntry entry)
    {
        lock (Entries)
        {
            Entries.Enqueue(entry);
            if (Entries.Count > Capacity)
            {
                Entries.Dequeue();
            }
        }
    }
}