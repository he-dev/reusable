namespace Reusable.Shelly
{
    public static class LoggerExtensions
    {
        public static ILogger Trace(this ILogger logger, string message)
        {
            logger.Log(new LogEntry { Message = message, LogLevel = LogLevel.Trace });
            return logger;
        }

        public static ILogger Debug(this ILogger logger, string message)
        {
            logger.Log(new LogEntry { Message = message, LogLevel = LogLevel.Debug });
            return logger;
        }

        public static ILogger Info(this ILogger logger, string message)
        {
            logger.Log(new LogEntry { Message = message, LogLevel = LogLevel.Info });
            return logger;
        }

        public static ILogger Warn(this ILogger logger, string message)
        {
            logger.Log(new LogEntry { Message = message, LogLevel = LogLevel.Warn });
            return logger;
        }

        public static ILogger Error(this ILogger logger, string message)
        {
            logger.Log(new LogEntry { Message = message, LogLevel = LogLevel.Error });
            return logger;
        }
    }
}
