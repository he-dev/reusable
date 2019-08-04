namespace Reusable.OmniLog.Abstractions.v2
{
    public interface ILogger
    {
        T Use<T>(T next) where T : LoggerMiddleware;

        void Log(ILog log);
    }
}