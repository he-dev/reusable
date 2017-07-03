namespace Reusable.CommandLine.Logging
{
    public interface ILogger
    {
        ILogger Log(string message, LogLevel logLevel);
    }
}