using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    [PublicAPI]
    public class MemoryRx : ILogRx, IEnumerable<ILogEntry>
    {
        public const int DefaultCapacity = 1_000;

        private readonly LinkedList<ILogEntry> _logs = new LinkedList<ILogEntry>();

        public MemoryRx(int capacity = DefaultCapacity)
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

        public static MemoryRx Create(int capacity = DefaultCapacity) => new MemoryRx(capacity);

        public IEnumerator<ILogEntry> GetEnumerator()
        {
            lock (_logs)
            {
                return _logs.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}