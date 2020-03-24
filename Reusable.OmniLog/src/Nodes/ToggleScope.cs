using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    public interface IFlowScope
    {
        ILoggerNode First { get; }

        IFlowScope Push(Exception exception);
    }

    [PublicAPI]
    public class ToggleScope : LoggerNode, IFlowScope
    {
        public static AsyncScope<Item>? Current => AsyncScope<Item>.Current;

        public override bool Enabled => AsyncScope<Item>.Any;

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; } = Enumerable.Empty<ILoggerNode>;

        private Item Scope => Current?.Value ?? throw new InvalidOperationException($"Cannot use {nameof(Scope)} when {nameof(ToggleScope)} is disabled. Use Logger.BeginScope() first.");

        public ILoggerNode First => Scope.First;

        public Action<ILogger, string> OnBeginScope { get; set; } = (logger, name) => logger.Log(Execution.Context.BeginScope(name));

        public Action<ILogger, Exception?> OnEndScope { get; set; } = (logger, exception) => logger.Log(Execution.Context.EndScope(exception));

        public IDisposable Push(string name)
        {
            try
            {
                return AsyncScope<Item>.Push(new Item(this, CreateNodes()) { OnEndScope = OnEndScope }).Value;
            }
            finally
            {
                OnBeginScope(Next.First().Node<Logger>(), name);
            }
        }

        public IFlowScope Push(Exception exception)
        {
            return this.Pipe(_ => AsyncScope<Item>.Current.Value.Exceptions.Push(exception));
        }

        public override void Invoke(ILogEntry request)
        {
            Scope.First.Invoke(request);
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

            public Action<ILogger, Exception> OnEndScope { get; set; } = (logger, exception) => { };

            public Stack<Exception> Exceptions { get; set; } = new Stack<Exception>();

            public void Dispose()
            {
                var exception =
                    Exceptions.Any()
                        ? Exceptions.Count > 1
                            ? new AggregateException(Exceptions)
                            : Exceptions.Peek()
                        : default;

                OnEndScope(First.First().Node<Logger>(), exception);

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

                Current?.Dispose();
            }

            private bool IsMainPipeline(ILoggerNode node)
            {
                return !ReferenceEquals(node, First) && node.Prev is ToggleScope;
            }
        }
    }

    public enum FlowStatus
    {
        Undefined,
        Begin,
        Completed,
        Canceled,
        Faulted
    }

    public static class InjectFlowScopeHelper
    {
        // [Obsolete("Use 'BeginScope'.")]
        // public static ILoggerScope UseScope(this ILogger logger, object? correlationId = default, object? correlationHandle = default)
        // {
        //     return logger.BeginScope(correlationId).WithCorrelationHandle(correlationHandle);
        // }

        public static ILoggerScope BeginScope(this ILogger logger, string name, object? correlationId = default, object? correlationHandle = default)
        {
            return new LoggerScope<ToggleScope>(logger, branch => branch.Push(name));
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static IFlowScope Scope(this ILogger logger) => logger.Node<ToggleScope>();
    }
}