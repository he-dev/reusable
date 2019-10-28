using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog
{
    public class Logger : ILogger
    {
        private readonly LoggerNode _node;

        public Logger(LoggerNode node)
        {
            // Always start with the first middleware.
            _node = node;
        }

        public LoggerNode Node => _node;

        //        public T Use<T>(T next) where T : LoggerNode
        //        {
        //            return (T)_node.InsertRelative(next, _middlewarePositions);
        //        }

        public void Log(LogEntry logEntry)
        {
            _node.Invoke(logEntry);
        }
    }

    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(T).ToPrettyString());
        }

        public static ILogger<T> Empty { get; } = new EmptyLogger();

        public LoggerNode Node => _logger.Node;

        //        public T1 Use<T1>(T1 next) where T1 : LoggerNode
        //        {
        //            return _logger.Use(next);
        //        }

        public void Log(LogEntry logEntry)
        {
            _logger.Log(logEntry);
        }

        private class EmptyLogger : ILogger<T>
        {
            public LoggerNode Node { get; }

            public void Log(LogEntry logEntry) { }
        }
    }

    public readonly struct LoggerScope<T> : ILoggerScope where T : LoggerNode
    {
        private readonly ILogger _logger;
        private readonly IDisposable _scope;

        public LoggerScope(ILogger logger, Func<T, IDisposable> push)
        {
            _logger = logger;
            _scope = push(logger.Node<T>());
        }

        #region ILogger

        public LoggerNode Node => _logger.Node;

        public void Log(LogEntry logEntry) => _logger.Log(logEntry);

        #endregion

        public void Dispose()
        {
            _scope.Dispose();
            if (_logger is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}