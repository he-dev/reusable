using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Rx
{
    public class NullRx : ILogRx
    {
        public void Log(LogEntry logEntry) { }
    }
}