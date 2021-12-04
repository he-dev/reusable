using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Connectors;

[PublicAPI]
public class MemoryConnector : IConnector, IEnumerable<ILogEntry>
{
    public const int DefaultCapacity = 1_000;

    private readonly LinkedList<ILogEntry> _logs = new();

    public MemoryConnector(int capacity = DefaultCapacity)
    {
        Capacity = capacity;
    }

    public int Capacity { get; }

    public ILogEntry? this[int index] => this.ElementAtOrDefault(index);

    public void Log(ILogEntry logEntry)
    {
        lock (_logs)
        {
            _logs.AddLast(logEntry);
            if (_logs.Count > Capacity)
            {
                _logs.RemoveFirst();
            }
        }
    }
        
    public IEnumerator<ILogEntry> GetEnumerator()
    {
        lock (_logs)
        {
            return _logs.GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}