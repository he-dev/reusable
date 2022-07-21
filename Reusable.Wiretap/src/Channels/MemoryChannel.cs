using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

[PublicAPI]
public class MemoryChannel : Channel, IEnumerable<ILogEntry>
{
    public const int DefaultCapacity = 1_000;

    private readonly LinkedList<ILogEntry> _logs = new();

    public MemoryChannel(int capacity = DefaultCapacity)
    {
        Capacity = capacity;
    }

    public int Capacity { get; }

    public ILogEntry? this[int index] => this.ElementAtOrDefault(index);

    public override void Invoke(ILogEntry entry)
    {
        lock (_logs)
        {
            _logs.AddLast(entry);
            if (_logs.Count > Capacity)
            {
                _logs.RemoveFirst();
            }
        }
        
        Next?.Invoke(entry);
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