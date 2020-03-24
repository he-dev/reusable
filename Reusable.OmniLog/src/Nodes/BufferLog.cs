using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.Extensions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Temporarily holding log-entries while it's waiting to be transferred to another location. This node is disabled by default. 
    /// </summary>
    public class BufferLog : LoggerNode
    {
        private readonly Queue<ILogEntry> _buffer = new Queue<ILogEntry>();

        public override bool Enabled { get; set; }

        public override void Invoke(ILogEntry request)
        {
            _buffer.Enqueue(request);
            // Don't call Next until Flush.
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