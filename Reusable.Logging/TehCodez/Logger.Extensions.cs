using System;

namespace Reusable.Logging.Loggex
{
    public static class LoggerExtensions
    {
        public static ILogger Log(this ILogger logger, Func<LogEntry, LogEntry> logEntry)
        {
            return logger.Log(logEntry(LogEntry.Create()));
        }

        public static (ILogger Logger, LogEntry LogEntry) BeginLog(this ILogger logger, Func<LogEntry, LogEntry> logEntry = null)
        {
            var e = logEntry?.Invoke(LogEntry.Create()) ?? LogEntry.Create();
            return (logger, e);
        }

        public static ILogger EndLog(this (ILogger Logger, LogEntry LogEntry) t)
        {
            return t.Logger.Log(t.LogEntry);
        }
    }
}