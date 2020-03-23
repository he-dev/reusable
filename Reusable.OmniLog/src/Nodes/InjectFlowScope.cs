using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    public interface IFlowScope
    {
        ILoggerNode First { get; }
    }
    
    [PublicAPI]
    public class InjectFlowScope : LoggerNode, IFlowScope
    {
        public static AsyncScope<Item>? Context => AsyncScope<Item>.Current;

        public override bool Enabled => AsyncScope<Item>.Any;

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; } = Enumerable.Empty<ILoggerNode>;

        private Item Scope => Context?.Value ?? throw new InvalidOperationException($"Cannot use {nameof(Scope)} when {nameof(InjectFlowScope)} is disabled. Use Logger.BeginScope() first.");

        public ILoggerNode First => Scope.First;

        public IDisposable Push()
        {
            return AsyncScope<Item>.Push(new Item(this, CreateNodes())).Value;
        }

        public override void Invoke(ILogEntry request)
        {
            Scope.First.Invoke(request);
        }

        public ILoggerNode Append(ILoggerNode node)
        {
            return Scope.First.Last().Append(node);
        }

        public class Item : IDisposable
        {
            public Item(ILoggerNode branch, IEnumerable<ILoggerNode> nodes)
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
                    if (IsMainPipeline(node))
                    {
                        break;
                    }
                    else
                    {
                        node.Dispose();
                    }
                }

                Context?.Dispose();
            }

            private bool IsMainPipeline(ILoggerNode node)
            {
                return !ReferenceEquals(node, First) && node.Prev is InjectFlowScope;
            }
        }
    }

    public static class InjectFlowScopeHelper
    {
        [Obsolete("Use 'BeginScope'.")]
        public static ILoggerScope UseScope(this ILogger logger, object? correlationId = default, object? correlationHandle = default)
        {
            return logger.BeginScope(correlationId).WithCorrelationHandle(correlationHandle);
        }

        public static ILoggerScope BeginScope(this ILogger logger, object? correlationId = default)
        {
            return new LoggerScope<InjectFlowScope>(logger, branch =>
            {
                try
                {
                    return branch.Push();
                }
                finally
                {
                    if (correlationId is {} && branch.First.NodeOrDefault<Correlate>() is {} correlation)
                    {
                        correlation.CorrelationId = correlationId;
                    }
                }
            });
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static IFlowScope Scope(this ILogger logger) => logger.Node<InjectFlowScope>();
    }
}