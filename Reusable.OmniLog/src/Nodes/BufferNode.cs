using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Temporarily holding log-entries while it's waiting to be transferred to another location. 
    /// </summary>
    public class BufferNode : LoggerNode
    {
        private readonly Queue<LogEntry> _buffer = new Queue<LogEntry>();

        protected override void invoke(LogEntry request)
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
        public static ILoggerScope UseBuffer(this ILoggerScope scope) => scope.AddMiddleware(new BufferNode());

        public static BufferNode? Buffer(this ScopeNode.Item? scope) => scope?.EnumerateNext().OfType<BufferNode>().SingleOrDefault();
        
        //public static IEnumerable<BufferNode> Buffers(this ILogger logger) => logger.Scope().Branch.EnumerateNext().OfType<BufferNode>();
    }
}