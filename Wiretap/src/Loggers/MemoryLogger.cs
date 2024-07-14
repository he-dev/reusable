using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Wiretap.Loggers;

[PublicAPI]
public class MemoryLogger(int capacity = 1_000_000) : ILogger, IEnumerable<IDictionary<string, object>>
{
    private Queue<IDictionary<string, object>> Entries { get; } = new();

    public void Log(IEnumerable<KeyValuePair<string, object>> properties)
    {
        lock (Entries)
        {
            Entries.Enqueue(properties.ToDictionary(x => x.Key, x => x.Value, new StringComparerLite()));
            if (capacity > 0 && Entries.Count > capacity)
            {
                Entries.Dequeue();
            }
        }
    }

    public IEnumerator<IDictionary<string, object>> GetEnumerator() => Entries.GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Entries).GetEnumerator();
}