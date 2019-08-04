using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerCorrelation : LoggerMiddleware, ILoggerScope<LoggerCorrelationScope, (object CorrelationId, object CorrelationHandle)>
    {
        /// <summary>
        /// Gets or sets the factory for the default correlation-id. By default it's a Guid.
        /// </summary>
        public Func<object> NextCorrelationId { get; set; } = () => Guid.NewGuid().ToString("N");

        public override bool IsActive => LoggerScope<LoggerCorrelationScope>.Current.Any();

        public LoggerCorrelationScope Push((object CorrelationId, object CorrelationHandle) parameter)
        {
            return LoggerScope<LoggerCorrelationScope>.Push(new LoggerCorrelationScope
            {
                CorrelationId = parameter.CorrelationId ?? NextCorrelationId()
            });
        }


        protected override void InvokeCore(ILog request)
        {
            request.AttachSerializable("Scope", LoggerScope<LoggerCorrelationScope>.Peek());
            Next?.Invoke(request);
        }
    }

    public class LoggerCorrelationScope : IDisposable
    {
        public object CorrelationId { get; set; }

        public object CorrelationHandle { get; set; }

        public void Dispose() => LoggerScope<LoggerCorrelationScope>.Pop();
    }
}