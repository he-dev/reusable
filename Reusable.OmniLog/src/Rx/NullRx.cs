using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Rx
{
    public class NullRx : ILogRx
    {
        public void Log(ILogEntry logEntry) { }
    }
}