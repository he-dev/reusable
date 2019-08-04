namespace Reusable.OmniLog.Abstractions.v2
{
    public interface ILogger
    {
        /// <summary>
        /// Gets middleware root.
        /// </summary>
        LoggerMiddleware Middleware { get; }

        T Use<T>(T next) where T : LoggerMiddleware;

        void Log(ILog log);
    }
}