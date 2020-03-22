using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    [PublicAPI]
    public class BranchNode : LoggerNode
    {
        public BranchNode()
        {
            AsyncScope<ILoggerNode>.Push(new CorrelationNode { CorrelationHandle = "Session" });
        }

        public static AsyncScope<Branch>? Scope => AsyncScope<Branch>.Current;

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
        public ILoggerNode First => AsyncScope<Branch>.Current?.Value.First ?? throw new InvalidOperationException($"Cannot use {nameof(First)} when {nameof(BranchNode)} is disabled. Use Logger.BeginScope() first.");

        public IDisposable Push()
        {
            return AsyncScope<Branch>.Push(new Branch(this, CreateNodes())).Value;
        }

        public override void Invoke(ILogEntry request)
        {
            First.Invoke(request);
        }

        public ILoggerNode Append(ILoggerNode node)
        {
            return First.Last().Append(node);
        }

        public class Branch : IDisposable
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

                Scope?.Dispose();
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

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static BranchNode Scope(this ILogger logger) => logger.Node<BranchNode>();

        
    }
}