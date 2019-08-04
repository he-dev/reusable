using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    public class LoggerEcho : LoggerMiddleware
    {
        private readonly IEnumerable<ILogRx> _receivers;

        public LoggerEcho(IEnumerable<ILogRx> receivers) : base(true)
        {
            _receivers = receivers;
        }

        protected override void InvokeCore(ILog request)
        {
            // todo - this isn't probably the best place for it
            if (!request.ContainsKey("Level"))
            {
                request.SetItem("Level", LogLevel.Information);
            }
            
            foreach (var rx in _receivers)
            {
                rx.Log(request);
            }
        }
    }
}