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

        public IBranchBuilder BranchBuilder { get; set; }

        public ILoggerNode? First => AsyncScope<Branch>.Current?.Value.First;

        public IDisposable Push()
        {
            return AsyncScope<Branch>.Push(new Branch { First = BranchBuilder.Build(this) }).Value;
        }

        public override void Invoke(ILogEntry request)
        {
            var branch = AsyncScope<Branch>.Current;
            var scopes = branch.Enumerate().Select(x => x.Value.First.Node<CorrelationNode>()).ToList();
            request.Add(LogProperty.Names.Correlation, scopes, m => m.ProcessWith<SerializerNode>());
            First.Invoke(request);
        }

        public ILoggerNode Append(ILoggerNode node)
        {
            return First.Last().Append(node);
        }

        internal class Branch : IDisposable
        {
            public ILoggerNode First { get; set; }

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

    public interface IBranchBuilder
    {
        ILoggerNode Build(ILoggerNode branch);
    }

    public class BranchBuilder : IBranchBuilder
    {
        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; }

        public ILoggerNode Build(ILoggerNode branch)
        {
            var last = CreateNodes().Join();
            last.Next = branch.Next;
            var first = last.First();
            first.Prev = branch.Prev;
            return first;
        }
    }


    public static class ScopeNodeHelper
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

        public static CorrelationNode Correlation(this BranchNode logger) => logger.First.Node<CorrelationNode>();
    }
}