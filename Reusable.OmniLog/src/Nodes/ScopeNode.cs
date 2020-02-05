using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    [PublicAPI]
    public class ScopeNode : LoggerNode //, ILoggerNodeScope<ScopeNode.Item, object>
    {
        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        public FirstNode? Current => AsyncScope<FirstNode>.Current?.Value;

        public FirstNode Push(object? correlationId)
        {
            return AsyncScope<FirstNode>.Push(new FirstNode(correlationId ?? NextCorrelationId(), Next)).Value;
        }

        protected override void invoke(LogEntry request)
        {
            if (AsyncScope<FirstNode>.Any)
            {
                var scopes = AsyncScope<FirstNode>.Current!.Enumerate().Select(x => x.Value).ToList();
                request.Add<Serialize>(LogEntry.Names.Scope, scopes);
                AsyncScope<FirstNode>.Current!.Value.Invoke(request);
            }
            else
            {
                invokeNext(request);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class FirstNode : ILoggerNode
        {
            public FirstNode(object correlationId, ILoggerNode? next) => (CorrelationId, Next) = (correlationId, new LastNode { Prev = this, Next = next });

            [JsonProperty]
            public object CorrelationId { get; }

            [JsonProperty]
            public object? CorrelationHandle { get; set; }

            public bool Enabled { get; set; } = true;

            public ILoggerNode? Prev { get; set; }

            public ILoggerNode? Next { get; set; }

            public void Invoke(LogEntry request) => Next?.Invoke(request);

            public void Dispose()
            {
                Next?.Dispose();
                AsyncScope<FirstNode>.Current?.Dispose();
            }
        }

        public class LastNode : LoggerNode
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
            correlationId = scope.Scope()!.CorrelationId;
            return scope;
        }

        public static ILoggerScope WithCorrelationHandle(this ILoggerScope scope, object? correlationHandle)
        {
            return scope.WithScope(s => s.CorrelationHandle = correlationHandle);
        }

        public static ILoggerScope AddNode(this ILoggerScope scope, ILoggerNode node)
        {
            return scope.WithScope(s => s.EnumerateNext().OfType<ScopeNode.LastNode>().Single().AddBefore(node));
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static ScopeNode.FirstNode? Scope(this ILogger logger) => logger.Node<ScopeNode>().Current;

        public static ILoggerScope WithScope(this ILoggerScope scope, Action<ScopeNode.FirstNode> scopeAction)
        {
            return scope.Pipe(s => scopeAction(s.Scope() ?? throw new InvalidOperationException("Cannot use scope right now because there is none.")));
        }
    }
}