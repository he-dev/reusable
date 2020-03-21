using System;
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

        public BufferNode()
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

    public static class BufferNodeHelper
    {
        public static ILoggerScope UseBuffer(this ILoggerScope logger)
        {
            // Branch-node is properly initialized at this point.
            return logger.Pipe(x => x.Node<BranchNode>().First.Node<BufferNode>().Enable());
        }

        public static BufferNode Buffer(this BranchNode logger)
        {
            if (!logger.Enabled)
            {
                throw new InvalidOperationException
                (
                    $"Cannot get {nameof(BufferNode)} because it is not initialized. Use Logger.BeginScope() or check whether it is registered in the {nameof(BranchNode)}."
                );
            }

            return logger.First.Node<BufferNode>();
        }
    }
}