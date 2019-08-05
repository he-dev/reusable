using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    public class LoggerFilter : LoggerMiddleware
    {
        private readonly Func<Log, bool> _canLog;

        public LoggerFilter(Func<Log, bool> canLog) : base(true)
        {
            _canLog = canLog;
        }

        protected override void InvokeCore(Log request)
        {
            if (_canLog(request))
            {
                Next?.Invoke(request);
            }
        }
    }
}