using System.Collections;
using System.Collections.Generic;

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