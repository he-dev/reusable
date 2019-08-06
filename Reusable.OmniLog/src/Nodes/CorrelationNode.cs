using System;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    public class CorrelationNode : LoggerNode, ILoggerScope<CorrelationNode.Scope, (object CorrelationId, object CorrelationHandle)>
    {
        public static readonly string DefaultPropertyName = "Correlation";
        
        public CorrelationNode() : base(false) { }

        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        public override bool IsActive => !LoggerScope<Scope>.IsEmpty;

        public string PropertyName { get; set; } = DefaultPropertyName;

        public Scope Push((object CorrelationId, object CorrelationHandle) parameter)
        {
            return LoggerScope<Scope>.Push(new Scope
            {
                CorrelationId = parameter.CorrelationId ?? NextCorrelationId(),
                CorrelationHandle = parameter.CorrelationHandle
            }).Value;
        }


        protected override void InvokeCore(LogEntry request)
        {
            request.Serializable(PropertyName, LoggerScope<Scope>.Current.Value);
            Next?.Invoke(request);
        }

        public class Scope : IDisposable
        {
            public object CorrelationId { get; set; }

            public object CorrelationHandle { get; set; }

            public void Dispose() => LoggerScope<Scope>.Current.Dispose();
        }
    }
    
    public static class LoggerCorrelationHelper
    {
        public static CorrelationNode.Scope UseScope(this ILogger logger, object correlationId = default, object correlationHandle = default)
        {
            return
                logger
                    .Node
                    .Enumerate(m => m.Next)
                    .OfType<CorrelationNode>()
                    .Single()
                    .Push((correlationId, correlationHandle));
        }
    }
}