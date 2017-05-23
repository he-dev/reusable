namespace Reusable.Colin.Logging
{
    public interface ILogger
    {
        ILogger Log(string message, LogLevel logLevel);
    }
}