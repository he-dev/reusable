using System;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    public class CorrelationNode : LoggerNode, ILoggerNodeScope<CorrelationNode.Scope, (object CorrelationId, object CorrelationHandle)>
    {
        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        public override bool Enabled => AsyncScope<Scope>.Any;

        public Scope Current => AsyncScope<Scope>.Current?.Value;
        
        public Scope Push((object CorrelationId, object CorrelationHandle) parameter)
        {
            return AsyncScope<Scope>.Push(new Scope
            {
                CorrelationId = parameter.CorrelationId ?? NextCorrelationId(),
                CorrelationHandle = parameter.CorrelationHandle
            }).Value;
        }

        protected override void invoke(LogEntry request)
        {
            request.SetItem(LogEntry.Names.Scope, LogEntry.Tags.Serializable, AsyncScope<Scope>.Current.Enumerate().Select(x => x.Value).ToList());
            Next?.Invoke(request);
        }

        public class Scope : IDisposable
        {
            public object CorrelationId { get; set; }

            public object CorrelationHandle { get; set; }

            public void Dispose() => AsyncScope<Scope>.Current.Dispose();
        }
    }

    public static class LoggerCorrelationHelper
    {
        // public static CorrelationNode.Scope UseScope(this ILogger logger, object correlationId = default, object correlationHandle = default)
        // {
        //     return
        //         logger
        //             .Node<CorrelationNode>()
        //             .Push((correlationId, correlationHandle));
        // }
        
        // Obsolete
        public static ILoggerScope UseScope(this ILogger logger, object correlationId = default, object correlationHandle = default)
        {
            return new LoggerScope<CorrelationNode>(logger, node => node.Push((correlationId, correlationHandle)));
        }
        
        public static ILoggerScope BeginScope(this ILogger logger)
        {
            return new LoggerScope<CorrelationNode>(logger, node => node.Push((default, default)));
        }
    }
}