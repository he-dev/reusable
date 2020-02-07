namespace Reusable.OmniLog.Abstractions
{
    public interface ILogger : ILoggerNode
    {
        void Log(ILogEntry logEntry);
    }

    // ReSharper disable once UnusedTypeParameter - This is required for dependency-injection.
    public interface ILogger<T> : ILogger { }

    public interface ILoggerScope : ILogger { }
}