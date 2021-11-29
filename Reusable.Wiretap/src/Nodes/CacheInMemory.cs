using System.Collections;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node caches logs in memory. It is disabled by default.
    /// </summary>
    public class CacheInMemory : LoggerNode, IEnumerable<ILogEntry>
    {
        private readonly Queue<ILogEntry> _entries = new Queue<ILogEntry>();

        public override bool Enabled { get; set; }

        public int Capacity { get; set; } = 10_000;

        public override void Invoke(ILogEntry request)
        {
            lock (_entries)
            {
                _entries.Enqueue(request);

                if (_entries.Count > Capacity)
                {
                    _entries.Dequeue();
                }
            }

            InvokeNext(request);
        }

        public IEnumerator<ILogEntry> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entries).GetEnumerator();

        public override void Dispose()
        {
            Enabled = false;
            _entries.Clear();
            base.Dispose();
        }
    }
}