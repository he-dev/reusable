using System;
using System.Collections.Generic;

namespace Reusable.Shelly
{
    public interface ILogger
    {
        LogLevel LogLevel { get; set; }
        void Log(LogEntry logEntry);
    }

    public abstract class Logger : ILogger
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        public static ILogger Empty => new CompositeLogger();

        public void Log(LogEntry logEntry)
        {
            if (logEntry.LogLevel >= LogLevel)
            {
                Write(logEntry);
            }
        }

        protected abstract void Write(LogEntry logEntry);
    }

    public class NullLogger : Logger
    {
        protected override void Write(LogEntry logEntry) { }
    }

    public class CompositeLogger : ILogger
    {
        private readonly IEnumerable<ILogger> _loggers;

        private LogLevel _logLevel = 0;

        internal CompositeLogger(params ILogger[] loggers)
        {
            _loggers = loggers;
        }

        public LogLevel LogLevel
        {
            get { return _logLevel; }
            set
            {
                _logLevel = value;
                ForEach(logger => logger.LogLevel = value);
            }
        }

        public void Log(LogEntry logEntry)
        {
            ForEach(logger => logger.Log(logEntry));
        }

        private void ForEach(Action<ILogger> loggerAction)
        {
            foreach (var logger in _loggers)
            {
                loggerAction(logger);
            };
        }
    }

    public static class LoggerComposition
    {
        public static ILogger Add<T>(this ILogger logger, LogLevel logLevel = LogLevel.Info) where T : ILogger, new()
        {
            return new CompositeLogger(logger, new T { LogLevel = logLevel });
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public DateTime TimestampUtc { get; } = DateTime.UtcNow;
        public string Message { get; internal set; }
        public LogLevel LogLevel { get; internal set; }
    }
}
