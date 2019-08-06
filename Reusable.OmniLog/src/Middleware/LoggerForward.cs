using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    // Reroutes items from one property to the other: Meta#Dump --> Snapshot#Dump 
    public class LoggerForward : LoggerMiddleware
    {
        public LoggerForward() : base(true) { }

        protected override void InvokeCore(Log request)
        {
            Next?.Invoke(request);
        }
    }
}