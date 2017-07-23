namespace Reusable.Logging.Loggex
{
    public interface ILogger
    {
        CaseInsensitiveString Name { get; }

        ILogger Log(LogEntry logEntry);
    }

    public partial class Logger : ILogger
    {
        public static  LoggerConfiguration Configuration { get; set; } = new LoggerConfiguration();

        public static ILogger Create(string name, LoggerConfiguration configuration)
        {
            return new Logger(name, configuration);
        }

        public static ILogger Create(string name)
        {
            return Create(name, Configuration);
        }

        public static ILogger Create<T>(LoggerConfiguration configuration)
        {
            return Create(typeof(T).Name, configuration);
        }

        public static ILogger Create<T>()
        {
            return Create(typeof(T).Name, Configuration);
        }
    }
}