using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class ScopeNode : LoggerNode, ILoggerNodeScope<ScopeNode.Scope, object>
    {
        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        //public override bool Enabled => AsyncScope<Scope>.Any;

        public Scope? Current => AsyncScope<Scope>.Current?.Value;

        public Scope Push(object? correlationId)
        {
            return AsyncScope<Scope>.Push(new Scope(correlationId ?? NextCorrelationId(), Next)).Value;
        }

        protected override void invoke(LogEntry request)
        {
            if (AsyncScope<Scope>.Any)
            {
                request.SetItem(LogEntry.Names.Scope, LogEntry.Tags.Serializable, AsyncScope<Scope>.Current.Enumerate().Select(x => x.Value).ToList());
                AsyncScope<Scope>.Current.Value.Next.Invoke(request);
            }
            else
            {
                invokeNext(request);
            }
        }

        public class Scope : IDisposable, IEnumerable<ILoggerNode>
        {
            public Scope(object correlationId, ILoggerNode next) => (CorrelationId, Next) = (correlationId, new TerminatorMiddleware().InsertAfter(next));

            public object CorrelationId { get; }

            public object? CorrelationHandle { get; set; }

            public ILoggerNode Next { get; set; }

            public void Dispose()
            {
                Next.Dispose();
                AsyncScope<Scope>.Current.Dispose();
            }

            public IEnumerator<ILoggerNode> GetEnumerator() => Next.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    public class TerminatorMiddleware : LoggerNode
    {
        protected override void invoke(LogEntry request)
        {
            invokeNext(request);
        }

        public override void Dispose()
        {
            // Terminate the Dispose chain.
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
            return new LoggerScope<ScopeNode>(logger, node => node.Push(correlationId));
        }

        public static ILoggerScope BeginScope(this ILogger logger, out object correlationId)
        {
            var scope = new LoggerScope<ScopeNode>(logger, node => node.Push(default));
            correlationId = scope.Scope().CorrelationId;
            return scope;
        }

        public static ILoggerScope WithCorrelationHandle(this ILoggerScope scope, object correlationHandle)
        {
            return scope.WithScope(s => s.CorrelationHandle = correlationHandle);
        }

        public static ILoggerScope AddMiddleware(this ILoggerScope scope, ILoggerNode node)
        {
            return scope.WithScope(s => node.InsertAfter(s.Next));
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static ScopeNode.Scope Scope(this ILogger logger) => logger.Node<ScopeNode>().Current;

        public static ILoggerScope WithScope(this ILoggerScope scope, Action<ScopeNode.Scope> scopeAction)
        {
            return scope.Do(s => scopeAction(s.Scope()));
        }
    }
}