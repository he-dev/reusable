using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.Extensions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Temporarily holding log-entries while it's waiting to be transferred to another location. 
    /// </summary>
    public class Buffer : LoggerNode
    {
        private readonly Queue<ILogEntry> _buffer = new Queue<ILogEntry>();

        public Buffer()
        {
            Enabled = false;
        }

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
            _buffer.Clear();
            base.Dispose();
        }
    }
}