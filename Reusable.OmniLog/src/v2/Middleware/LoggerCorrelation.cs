using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerCorrelation : LoggerMiddleware, ILoggerScope<LoggerCorrelation.Scope, (object CorrelationId, object CorrelationHandle)>
    {
        public LoggerCorrelation() : base(false) { }

        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        public override bool IsActive => !(LoggerScope<Scope>.Current is null);

        public Scope Push((object CorrelationId, object CorrelationHandle) parameter)
        {
            return LoggerScope<Scope>.Push(new Scope
            {
                CorrelationId = parameter.CorrelationId ?? NextCorrelationId(),
                CorrelationHandle = parameter.CorrelationHandle
            }).Value;
        }


        protected override void InvokeCore(ILog request)
        {
            request.AttachSerializable("Scope", LoggerScope<Scope>.Current.Value);
            Next?.Invoke(request);
        }

        public class Scope : IDisposable
        {
            public object CorrelationId { get; set; }

            public object CorrelationHandle { get; set; }

            public void Dispose() => LoggerScope<Scope>.Current.Dispose();
        }
    }
}