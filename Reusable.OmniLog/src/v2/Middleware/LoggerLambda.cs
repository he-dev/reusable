using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware {
    public class LoggerLambda : LoggerMiddleware
    {
        private readonly Action<ILog> _transform;

        public LoggerLambda(Action<ILog> transform)
        {
            _transform = transform;
        }

        protected override void InvokeCore(ILog request)
        {
            _transform(request);
            Next?.Invoke(request);
        }
    }
}