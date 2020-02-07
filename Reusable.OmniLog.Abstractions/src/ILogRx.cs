using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogRx
    {
        void Log(LogEntry logEntry);
    }
}