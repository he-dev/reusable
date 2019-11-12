using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog
{
    public class Logger : LoggerNode, ILogger
    {
        public virtual void Log(LogEntry logEntry) => invokeNext(logEntry);

        protected override void invoke(LogEntry request) => Log(request);
    }

    public class Logger<T> : Logger, ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(T).ToPrettyString());
        }

        public static ILogger<T> Empty { get; } = new EmptyLogger();

        public override void Log(LogEntry logEntry) => _logger.Log(logEntry);

        private class EmptyLogger : Logger, ILogger<T> { }
    }

    public class LoggerScope<T> : Logger, ILoggerScope where T : ILoggerNode
    {
        private readonly ILogger _logger;
        private readonly IDisposable _scope;

        public LoggerScope(ILogger logger, Func<T, IDisposable> configureNode)
        {
            _logger = logger;
            _scope = configureNode(logger.Node<T>());
        }

        public override void Log(LogEntry logEntry) => _logger.Log(logEntry);

        public override void Dispose() => _scope.Dispose();
    }
}