using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
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
                log = log.Flatten();
                var entry = new Log
                {
                    ["Level"] = log.Level(),
                    ["Logger"] = log.Name().ToString(),
                    ["Message"] = log.Message(),
                    ["Exception"] = log.Exception(),
                    ["TimeStamp"] = log.Timestamp()
                };

                foreach (var item in log)
                {
                    if (!entry.ContainsKey(item.Key.ToString()))
                    {
                        entry[item.Key.ToString()] = item.Value;
                    }
                }
                
                _logs.AddLast(entry);
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