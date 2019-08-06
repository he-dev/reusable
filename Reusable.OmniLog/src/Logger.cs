using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog
{
    public class Logger : ILogger
    {
        private readonly LoggerMiddleware _middleware;
        private readonly IDictionary<Type, int> _middlewarePositions;

        public Logger(LoggerMiddleware middleware, IDictionary<Type, int> middlewarePositions)
        {
            // Always start with the first middleware.
            _middleware = middleware;
            _middlewarePositions = middlewarePositions;
        }

        public LoggerMiddleware Middleware => _middleware;

        public T Use<T>(T next) where T : LoggerMiddleware
        {
            return (T)_middleware.InsertRelative(next, _middlewarePositions);
        }

        public void Log(LogEntry logEntry)
        {
            _middleware.Invoke(logEntry);
        }
    }

    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger;
        
        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(T).ToPrettyString());
        }

        public LoggerMiddleware Middleware => _logger.Middleware;
        
        public T1 Use<T1>(T1 next) where T1 : LoggerMiddleware
        {
            return _logger.Use(next);
        }

        public void Log(LogEntry logEntry)
        {
            _logger.Log(logEntry);
        }
    }
}