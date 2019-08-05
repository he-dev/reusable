using System;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerFilter : LoggerMiddleware
    {
        private readonly Func<ILog, bool> _canLog;

        public LoggerFilter(Func<ILog, bool> canLog) : base(true)
        {
            _canLog = canLog;
        }

        protected override void InvokeCore(Abstractions.v2.Log request)
        {
            if (_canLog(request))
            {
                Next?.Invoke(request);
            }
        }
    }
}