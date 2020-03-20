using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.Extensions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Temporarily holding log-entries while it's waiting to be transferred to another location. 
    /// </summary>
    public class BufferNode : LoggerNode
    {
        private readonly Queue<ILogEntry> _buffer = new Queue<ILogEntry>();

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

    public static class BufferNodeHelper
    {
        public static ILoggerScope UseBuffer(this ILoggerScope scope) => scope.Append(new BufferNode());

        public static BufferNode Buffer(this ScopeNode logger) => logger.First.Node<BufferNode>();
    }
}