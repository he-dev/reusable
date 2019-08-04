using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2
{
    public interface ILogger
    {
        /// <summary>
        /// Gets middleware root.
        /// </summary>
        LoggerMiddleware Middleware { get; }

        T Use<T>(T next) where T : LoggerMiddleware;

        void Log(ILog log);
    }

    public class Logger : ILogger
    {
        private readonly LoggerMiddleware _middleware;
        private readonly IDictionary<Type, int> _middlewarePositions;

        public Logger(LoggerMiddleware middleware, IDictionary<Type, int> middlewarePositions)
        {
            // Always start with the first middleware.
            _middleware = middleware.First();
            _middlewarePositions = middlewarePositions;
        }

        public LoggerMiddleware Middleware => _middleware;

        public T Use<T>(T next) where T : LoggerMiddleware
        {
            return (T)_middleware.InsertRelative(next, _middlewarePositions);
        }

        public void Log(ILog log)
        {
            _middleware.Invoke(log);
        }
    }

    public static class LoggerExtensions
    {
        public static void Log(this ILogger logger, Action<ILog> transform)
        {
            logger.UseLambda(transform);
            logger.Log(new Log());
        }
    }
}