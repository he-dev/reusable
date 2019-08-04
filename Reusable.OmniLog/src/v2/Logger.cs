using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2
{
    using Reusable.OmniLog.Abstractions.v2;

//    public interface ILogger
//    {
//        /// <summary>
//        /// Gets middleware root.
//        /// </summary>
//        LoggerMiddleware Middleware { get; }
//
//        T Use<T>(T next) where T : LoggerMiddleware;
//
//        void Log(ILog log);
//    }

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
        
        #region LogLevels

        public static ILogger Trace(this ILogger logger, string message, Func<ILog, ILog> populate = null)
        {
            return logger.Log(LogLevel.Trace, message, null, populate);
        }

        public static ILogger Debug(this ILogger logger, string message, Func<ILog, ILog> populate = null)
        {
            return logger.Log(LogLevel.Debug, message, null, populate);
        }

        public static ILogger Warning(this ILogger logger, string message, Func<ILog, ILog> populate = null)
        {
            return logger.Log(LogLevel.Warning, message, null, populate);
        }

        public static ILogger Information(this ILogger logger, string message, Func<ILog, ILog> populate = null)
        {
            return logger.Log(LogLevel.Information, message, null, populate);
        }

        public static ILogger Error(this ILogger logger, string message, Exception exception = null, Func<ILog, ILog> populate = null)
        {
            return logger.Log(LogLevel.Error, message, exception, populate);
        }

        public static ILogger Fatal(this ILogger logger, string message, Exception exception = null, Func<ILog, ILog> populate = null)
        {
            return logger.Log(LogLevel.Fatal, message, exception, populate);
        }

        private static ILogger Log
        (
            [NotNull] this ILogger logger,
            [NotNull] LogLevel logLevel,
            [CanBeNull] string message,
            [CanBeNull] Exception exception,
            [CanBeNull] Func<ILog, ILog> populate
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));

            logger.Log(log =>
            {
                log.SetItem(LogPropertyNames.Level, logLevel);
                log.SetItem(LogPropertyNames.Message, message);
                log.SetItem(LogPropertyNames.Exception, exception);
                //log.Transform(populate ?? Functional.Echo));
            });

            return logger;
        }

        #endregion
    }
}