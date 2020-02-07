namespace Reusable.OmniLog.Abstractions
{
    public interface ILogRx
    {
        void Log(ILogEntry logEntry);
    }
}