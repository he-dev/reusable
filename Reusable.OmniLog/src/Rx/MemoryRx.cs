using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Rx
{
    [PublicAPI]
    public class MemoryRx : ILogRx, IEnumerable<LogEntry>
    {
        public const int DefaultCapacity = 1_000;

        private readonly LinkedList<LogEntry> _logs = new LinkedList<LogEntry>();

        public MemoryRx(int capacity = DefaultCapacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }

        public LogEntry? this[int index] => this.ElementAtOrDefault(index);// ?? throw DynamicException.Create("LogIndexOutOfRange", $"There is no log at {index}.");

        public void Log(LogEntry logEntry)
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

        public IEnumerator<LogEntry> GetEnumerator()
        {
            lock (_logs)
            {
                return _logs.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}