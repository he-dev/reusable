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

    public class LoggerScope<T> : ILoggerScope where T : ILoggerNode
    {
        private readonly ILogger _logger;
        private readonly IDisposable _scope;

        public LoggerScope(ILogger logger, Func<T, IDisposable> configureNode)
        {
            _logger = logger;
            _scope = configureNode(logger.Node<T>());
        }

        public void Log(LogEntry logEntry) => _logger.Log(logEntry);

        public bool Enabled
        {
            get => _logger.Enabled;
            set => _logger.Enabled = value;
        }

        public ILoggerNode? Prev
        {
            get => _logger.Prev;
            set => _logger.Prev = value;
        }

        public ILoggerNode? Next
        {
            get => _logger.Next;
            set => _logger.Next = value;
        }

        public void Invoke(LogEntry request)
        {
            _logger.Invoke(request);
        }

        public void Dispose() => _scope.Dispose();
    }
}