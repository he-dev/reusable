using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    public class LoggerFilter : LoggerMiddleware
    {
        private readonly Func<LogEntry, bool> _canLog;

        public LoggerFilter(Func<LogEntry, bool> canLog) : base(true)
        {
            _canLog = canLog;
        }

        protected override void InvokeCore(LogEntry request)
        {
            if (_canLog(request))
            {
                Next?.Invoke(request);
            }
        }
    }
}