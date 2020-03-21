using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;

namespace Reusable.OmniLog.Nodes
{
    [PublicAPI]
    public class BranchNode : LoggerNode
    {
        public BranchNode()
        {
            AsyncScope<ILoggerNode>.Push(new CorrelationNode { CorrelationHandle = "Session" });
        }

        public override bool Enabled => AsyncScope<Branch>.Any;

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; } = () => new ILoggerNode[]
        {
            new CorrelationNode(),
            new StopwatchNode(),
            new BufferNode(),
            new MemoryNode(),
            new WorkItemNode(),
        };

        /// <summary>
        /// Gets the first node of the current branch.
        /// </summary>
        public ILoggerNode First => AsyncScope<Branch>.Current?.Value.First ?? throw new InvalidOperationException($"Cannot use {nameof(First)} when {nameof(BranchNode)} is disabled.");

        public IDisposable Push()
        {
            return AsyncScope<Branch>.Push(new Branch(this, CreateNodes())).Value;
        }

        public override void Invoke(ILogEntry request)
        {
            var branch = AsyncScope<Branch>.Current;
            var scopes = branch.Enumerate().Select(x => x.Value.First.Node<CorrelationNode>()).ToList();
            request.Add(Names.Default.Correlation, scopes, m => m.ProcessWith<SerializerNode>());
            First.Invoke(request); // This is guaranteed to be non-null here because otherwise this node is disabled.
        }

        public ILoggerNode Append(ILoggerNode node)
        {
            return First.Last().Append(node); // This is guaranteed to be non-null here because at this point the node is properly initialized.
        }

        internal class Branch : IDisposable
        {
            public Branch(ILoggerNode branch, IEnumerable<ILoggerNode> nodes)
            {
                var last = nodes.Join();
                last.Next = branch.Next;
                First = last.First();
                First.Prev = branch.Prev;
            }

            public ILoggerNode First { get; }

            public void Dispose()
            {
                foreach (var node in First.EnumerateNext())
                {
                    if (IsMainBranch(node))
                    {
                        break;
                    }
                    else
                    {
                        node.Dispose();
                    }
                }

                AsyncScope<Branch>.Current?.Dispose();
            }

            private bool IsMainBranch(ILoggerNode node)
            {
                return !ReferenceEquals(node, First) && node.Prev is BranchNode;
            }
        }
    }

    public static class BranchNodeHelper
    {
        [Obsolete("Use 'BeginScope'.")]
        public static ILoggerScope UseScope(this ILogger logger, object? correlationId = default, object? correlationHandle = default)
        {
            return logger.BeginScope(correlationId).WithCorrelationHandle(correlationHandle);
        }

        public static ILoggerScope BeginScope(this ILogger logger, object? correlationId = default)
        {
            return new LoggerScope<BranchNode>(logger, node => node.Push());
        }

        public static ILoggerScope WithCorrelationHandle(this ILoggerScope logger, object? correlationHandle)
        {
            return logger.Pipe(l => l.Scope().Correlation().CorrelationHandle = correlationHandle);
        }

        public static ILoggerScope Append(this ILoggerScope logger, ILoggerNode node)
        {
            return logger.Pipe(l => l.Scope().Append(node));
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static BranchNode Scope(this ILogger logger) => logger.Node<BranchNode>();

        public static CorrelationNode Correlation(this BranchNode logger)
        {
            if (!logger.Enabled)
            {
                throw new InvalidOperationException($"Cannot get {nameof(CorrelationNode)} because there is no scope. Use Logger.BeginScope() first.");
            }

            return logger.First.Node<CorrelationNode>();
        }
    }
}