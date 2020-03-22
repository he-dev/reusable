namespace Reusable.OmniLog.Abstractions
{
    public interface IConnector
    {
        void Log(ILogEntry logEntry);
    }
}