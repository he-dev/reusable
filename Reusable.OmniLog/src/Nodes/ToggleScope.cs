using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    [PublicAPI]
    public class ToggleScope : LoggerNode
    {
        public override bool Enabled => AsyncScope<Pipeline>.Any;

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; } = Enumerable.Empty<ILoggerNode>;

        public Pipeline Current => AsyncScope<Pipeline>.Current?.Value ?? throw new InvalidOperationException($"Cannot use {nameof(Current)} when {nameof(ToggleScope)} is disabled. Use Logger.BeginScope() first.");

        public Action<ILogger, string> OnBeginScope { get; set; } = (logger, name) => logger.Log(Execution.Context.BeginScope(name));

        public Action<ILogger, Exception?> OnEndScope { get; set; } = (logger, exception) => logger.Log(Execution.Context.EndScope(exception));

        public Stack<Exception> Exceptions { get; set; } = new Stack<Exception>();

        public IDisposable Push(string name)
        {
            try
            {
                return AsyncScope<Pipeline>.Push(new Pipeline(this, CreateNodes()) { }).Value;
            }
            finally
            {
                OnBeginScope(Next.First().Node<Logger>(), name);
            }
        }

        public override void Invoke(ILogEntry request)
        {
            Current.First.Invoke(request);
        }

        public class Pipeline : IEnumerable<ILoggerNode>, IDisposable
        {
            public Pipeline(ILoggerNode branch, IEnumerable<ILoggerNode> nodes)
            {
                var last = nodes.Join();
                last.Next = branch.Next;
                First = last.First();
                First.Prev = branch.Prev;
            }

            public ILoggerNode First { get; }

            public void Dispose()
            {
                foreach (var node in this)
                {
                    node.Dispose();
                }

                AsyncScope<Pipeline>.Current.Dispose();
            }

            private bool IsMainPipeline(ILoggerNode node)
            {
                return !ReferenceEquals(node, First) && node.Prev is ToggleScope;
            }

            public IEnumerator<ILoggerNode> GetEnumerator()
            {
                return First.EnumerateNext().TakeWhile(node => !IsMainPipeline(node)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public override void Dispose()
        {
            var exception =
                Exceptions.Any()
                    ? Exceptions.Count > 1
                        ? new AggregateException(Exceptions)
                        : Exceptions.Peek()
                    : default;

            OnEndScope(((ILoggerNode)this).First().Node<Logger>(), exception);
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

    public static class ToggleScopeHelper
    {
        // [Obsolete("Use 'BeginScope'.")]
        // public static ILoggerScope UseScope(this ILogger logger, object? correlationId = default, object? correlationHandle = default)
        // {
        //     return logger.BeginScope(correlationId).WithCorrelationHandle(correlationHandle);
        // }

        public static ILoggerScope BeginScope(this ILogger logger, string name)
        {
            return new LoggerScope<ToggleScope>(logger, branch => branch.Push(name));
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static ToggleScope.Pipeline Scope(this ILogger logger)
        {
            return logger.Node<ToggleScope>().Current;
        }
    }
}