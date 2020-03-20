using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;

namespace Reusable.OmniLog.Nodes
{
    [PublicAPI]
    public class ScopeNode : LoggerNode
    {
        public ScopeNode()
        {
            AsyncScope<ILoggerNode>.Push(new CorrelationNode(default) { CorrelationHandle = "Session" });
        }

        public ILoggerNode First => AsyncScope<ILoggerNode>.Current!.Value;

        public IDisposable Push(object? correlationId)
        {
            return AsyncScope<ILoggerNode>.Push(new CorrelationNode(correlationId) { Prev = Prev }).Value;
        }

        public override void Invoke(ILogEntry request)
        {
            var scopes = AsyncScope<ILoggerNode>.Current!.Enumerate().Select(x => x.Value.Node<CorrelationNode>()).ToList();
            request.Add(LogProperty.Names.Correlation, scopes, m => m.ProcessWith<SerializerNode>());
            First.Invoke(request);
            InvokeNext(request);
        }

        public ILoggerNode Append(ILoggerNode node)
        {
            return First.Last().Append(node);
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class CorrelationNode : LoggerNode
        {
            public CorrelationNode(object? correlationId) => CorrelationId = correlationId ?? NewCorrelationId();

            /// <summary>
            /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
            /// </summary>
            public Func<object> NewCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

            [JsonProperty]
            public object CorrelationId { get; }

            [JsonProperty]
            public object? CorrelationHandle { get; set; }

            //[JsonProperty]
            public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

            public override void Invoke(ILogEntry request) => InvokeNext(request);
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

        public static ILoggerScope WithCorrelationHandle(this ILoggerScope logger, object? correlationHandle)
        {
            return logger.Pipe(l => l.Scope().First.Node<ScopeNode.CorrelationNode>().CorrelationHandle = correlationHandle);
        }

        public static ILoggerScope Append(this ILoggerScope logger, ILoggerNode node)
        {
            return logger.Pipe(l => l.Scope().Append(node));
        }

        /// <summary>
        /// Gets the current correlation scope.
        /// </summary>
        public static ScopeNode Scope(this ILogger logger) => logger.Node<ScopeNode>();

        public static ScopeNode.CorrelationNode Correlation(this ILoggerNode logger) => logger.Node<ScopeNode.CorrelationNode>();
    }
}