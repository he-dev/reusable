using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog
{
    public class LoggerScope<T> : ILoggerScope where T : ILoggerNode
    {
        private readonly ILogger _logger;
        private readonly IDisposable _scope;

        public LoggerScope(ILogger logger, Func<T, IDisposable> configureNode)
        {
            _logger = logger;
            _scope = configureNode(logger.Node<T>());
        }

        public void Log(ILogEntry logEntry) => _logger.Log(logEntry);

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

        public void Invoke(ILogEntry request) => _logger.Invoke(request);

        public void Dispose() => _scope.Dispose();
    }
}