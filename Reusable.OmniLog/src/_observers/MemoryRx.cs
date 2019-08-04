using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class MemoryRx : LogRx, IEnumerable<ILog>
    {
        public const int DefaultCapacity = 1_000;

        private readonly LinkedList<ILog> _logs = new LinkedList<ILog>();

        public MemoryRx(int capacity = DefaultCapacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; }

        [NotNull]
        public ILog this[int index] => this.ElementAtOrDefault(index) ?? throw DynamicException.Create("LogIndexOutOfRange", $"There is no log at {index}.");

        public override void Log(ILog log)
        {
            lock (_logs)
            {
                log = log.Flatten();
                var entry = new Log
                {
                    ["Level"] = log.GetItemOrDefault<LogLevel>(Reusable.OmniLog.Log.PropertyNames.Level),
                    ["Logger"] = log.GetItemOrDefault<string>(Reusable.OmniLog.Log.PropertyNames.Logger),
                    ["Message"] = log.GetItemOrDefault<string>(Reusable.OmniLog.Log.PropertyNames.Message),
                    ["Exception"] = log.GetItemOrDefault<Exception>(Reusable.OmniLog.Log.PropertyNames.Exception),
                    ["TimeStamp"] = log.GetItemOrDefault<DateTime>(Reusable.OmniLog.Log.PropertyNames.Timestamp)
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

        public IEnumerator<ILog> GetEnumerator()
        {
            lock (_logs)
            {
                return _logs.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}