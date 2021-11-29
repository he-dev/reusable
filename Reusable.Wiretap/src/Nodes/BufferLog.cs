using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes
{
    /// <summary>
    /// This node temporarily stores log-entries. You need to use Flush to log empty the buffer. This node is disabled by default. 
    /// </summary>
    public class BufferLog : LoggerNode
    {
        private readonly Queue<ILogEntry> _buffer = new Queue<ILogEntry>();

        public override bool Enabled { get; set; }

        public override void Invoke(ILogEntry entry)
        {
            if (entry.TryGetProperty(Names.Properties.Priority, out var property) && property.Value.Equals(LogEntryPriority.High))
            {
                InvokeNext(entry);
            }
            else
            {
                _buffer.Enqueue(entry);
                // Don't call Next until Flush.
            }
        }

        public int Count => _buffer.Count;

        public void Flush()
        {
            foreach (var item in _buffer.Consume())
            {
                InvokeNext(item);
            }
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        public override void Dispose()
        {
            Enabled = false;
            _buffer.Clear();
            base.Dispose();
        }
    }
}