using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Temporarily holding log-entries while it's waiting to be transferred to another location. 
    /// </summary>
    public class BufferNode : LoggerNode
    {
        private readonly Queue<ILogEntry> _buffer = new Queue<ILogEntry>();

        protected override void invoke(ILogEntry request)
        {
            _buffer.Enqueue(request);
            // Don't call Next until Flush.
        }

        public int Count => _buffer.Count;

        public void Flush()
        {
            while (_buffer.Any())
            {
                invokeNext(_buffer.Dequeue());
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
        public static ILoggerScope UseBuffer(this ILoggerScope scope) => scope.AddNode(new BufferNode());

        public static BufferNode? Buffer(this ScopeNode.FirstNode? scope) => scope?.EnumerateNext().OfType<BufferNode>().SingleOrDefault();
    }
}