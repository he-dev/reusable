using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Wiretap.Loggers;

[PublicAPI]
public class MemoryLogger(int capacity = 1_000_000) : ILogger, IEnumerable<IDictionary<string, LogProperty>>
{
    private Queue<IDictionary<string, LogProperty>> Entries { get; } = new();

    public void Log(IEnumerable<LogProperty> properties)
    {
        lock (Entries)
        {
            Entries.Enqueue(properties.ToDictionary(x => x.Name, new StringComparerLite()));
            if (capacity > 0 && Entries.Count > capacity)
            {
                Entries.Dequeue();
            }
        }
    }

    public IEnumerator<IDictionary<string, LogProperty>> GetEnumerator() => Entries.GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Entries).GetEnumerator();
}