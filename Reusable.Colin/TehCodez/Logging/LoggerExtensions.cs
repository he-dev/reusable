using JetBrains.Annotations;

namespace Reusable.CommandLine.Logging
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        public static ILogger Debug(this ILogger logger, string message) => logger.Log(message, LogLevel.Debug);
        public static ILogger Info(this ILogger logger, string message) => logger.Log(message, LogLevel.Info);
        public static ILogger Warn(this ILogger logger, string message) => logger.Log(message, LogLevel.Warn);
        public static ILogger Error(this ILogger logger, string message) => logger.Log(message, LogLevel.Error);
    }
}