using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogger
    {
        /// <summary>
        /// Gets middleware root.
        /// </summary>
        LoggerNode Node { get; }

        //T Use<T>(T next) where T : LoggerNode;

        void Log(LogEntry logEntry);
    }

    public interface ILogger<T> : ILogger { }
}