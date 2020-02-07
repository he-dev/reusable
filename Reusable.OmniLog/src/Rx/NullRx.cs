using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class NullRx : ILogRx
    {
        public void Log(ILogEntry logEntry) { }
    }
}