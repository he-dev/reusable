using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware {
    public class LoggerFilter : LoggerMiddleware
    {
        private readonly Func<ILog, bool> _canLog;

        public LoggerFilter(Func<ILog, bool> canLog)
        {
            _canLog = canLog;
        }

        protected override void InvokeCore(ILog request)
        {
            if (_canLog(request))
            {
                Next?.Invoke(request);
            }
        }
    }
}