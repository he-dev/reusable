using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.OmniLog
{
    public class MemoryRx : LogRx, IEnumerable<Log>
    {
        public const int DefaultCapacity = 1_000;

        private readonly LinkedList<Log> _logs = new LinkedList<Log>();

        public MemoryRx(int capacity = DefaultCapacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }

        [NotNull]
        public Log this[int index] => this.ElementAtOrDefault(index) ?? throw DynamicException.Create("LogIndexOutOfRange", $"There is no log at {index}.");

        protected override void Log(Log log)
        {
            lock (_logs)
            {
                _logs.AddLast(log);
                if (_logs.Count > Capacity)
                {
                    _logs.RemoveFirst();
                }
            }
        }

        public static MemoryRx Create(int capacity = DefaultCapacity) => new MemoryRx(capacity);

        public IEnumerator<Log> GetEnumerator()
        {
            lock (_logs)
            {
                return _logs.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}