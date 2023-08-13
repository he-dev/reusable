using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Modules.Loggers;

[PublicAPI]
public class LogToMemory : ILog
{
    public const int DefaultCapacity = 1_000_000;

    public Queue<IDictionary<string, object?>> Entries { get; } = new();

    public int Capacity { get; set; } = DefaultCapacity;
    
    public void Invoke(TraceContext context, LogFunc next)
    {
        lock (Entries)
        {
            Entries.Enqueue(context.Entry);
            if (Entries.Count > Capacity)
            {
                Entries.Dequeue();
            }
        }

        next(context);
    }
}