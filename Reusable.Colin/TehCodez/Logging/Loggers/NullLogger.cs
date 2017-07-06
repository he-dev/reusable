namespace Reusable.CommandLine.Logging.Loggers
{
    public class NullLogger : ILogger
    {
        public ILogger Log(string message, LogLevel logLevel)
        {
            // does nothing
            return this;
        }
    }
}