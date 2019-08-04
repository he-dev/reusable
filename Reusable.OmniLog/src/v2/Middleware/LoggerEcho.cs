using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware {
    public class LoggerEcho : LoggerMiddleware
    {
        private readonly IEnumerable<ILogRx> _receivers;

        public LoggerEcho(IEnumerable<ILogRx> receivers)
        {
            _receivers = receivers;
        }

        protected override void InvokeCore(ILog request)
        {
            foreach (var rx in _receivers)
            {
                rx.Log(request);
            }
        }
    }
}