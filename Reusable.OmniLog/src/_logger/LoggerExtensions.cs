using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public delegate string MessageFunc();

    [PublicAPI]
    public static class LoggerExtensions
    {
        #region LogLevels

        public static ILogger Trace(this ILogger logger, string message, Func<ILog, ILog> logAction = null)
        {
            return logger.Log(LogLevel.Trace, message, null, logAction);
        }

        public static ILogger Debug(this ILogger logger, string message, Func<ILog, ILog> logAction = null)
        {
            return logger.Log(LogLevel.Debug, message, null, logAction);
        }

        public static ILogger Warning(this ILogger logger, string message, Func<ILog, ILog> logAction = null)
        {
            return logger.Log(LogLevel.Warning, message, null, logAction);
        }

        public static ILogger Information(this ILogger logger, string message, Func<ILog, ILog> logAction = null)
        {
            return logger.Log(LogLevel.Information, message, null, logAction);
        }

        public static ILogger Error(this ILogger logger, string message, Exception exception = null, Func<ILog, ILog> logAction = null)
        {
            return logger.Log(LogLevel.Error, message, exception, logAction);
        }

        public static ILogger Fatal(this ILogger logger, string message, Exception exception = null, Func<ILog, ILog> logAction = null)
        {
            return logger.Log(LogLevel.Fatal, message, exception, logAction);
        }

        private static ILogger Log
        (
            [NotNull] this ILogger logger,
            [NotNull] LogLevel logLevel,
            [CanBeNull] string message,
            [CanBeNull] Exception exception,
            [CanBeNull] Func<ILog, ILog> logAction
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));

            return logger.Log(log =>
            {
                log.With(LogProperties.Level, logLevel);
                log.Message(message);
                log.Exception(exception);
                logAction?.Invoke(log);
                return log;
            });
        }

        #endregion

        #region LogLevels lazy

        public static ILogger Trace(this ILogger logger, MessageFunc messageFunc, Func<ILog, ILog> logFunc = null)
        {
            return logger.Log(LogLevel.Trace, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Debug(this ILogger logger, MessageFunc messageFunc, Func<ILog, ILog> logFunc = null)
        {
            return logger.Log(LogLevel.Debug, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Information(this ILogger logger, MessageFunc messageFunc, Func<ILog, ILog> logFunc = null)
        {
            return logger.Log(LogLevel.Information, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Warning(this ILogger logger, MessageFunc messageFunc, Func<ILog, ILog> logFunc = null)
        {
            return logger.Log(LogLevel.Warning, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Error(this ILogger logger, MessageFunc messageFunc, Func<ILog, ILog> logFunc = null)
        {
            return logger.Log(LogLevel.Error, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Fatal(this ILogger logger, MessageFunc messageFunc, Func<ILog, ILog> logFunc = null)
        {
            return logger.Log(LogLevel.Fatal, messageFunc, logFunc ?? (_ => _));
        }

        public static T TraceReturn<T>(this ILogger logger, T obj, Func<T, string> messageFunc, Func<ILog, ILog> logFunc = null)
        {
            logger.Log(LogLevel.Trace, () => messageFunc(obj), logFunc ?? (_ => _));
            return obj;
        }

        public static T DebugReturn<T>(this ILogger logger, T obj, Func<T, string> messageFunc, Func<ILog, ILog> logFunc = null)
        {
            logger.Log(LogLevel.Debug, () => messageFunc(obj), logFunc ?? (_ => _));
            return obj;
        }

        private static ILogger Log
        (
            [NotNull] this ILogger logger,
            [NotNull] LogLevel logLevel,
            [NotNull] MessageFunc messageFunc,
            [NotNull] Func<ILog, ILog> logFunc
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));
            if (messageFunc == null) throw new ArgumentNullException(nameof(messageFunc));
            if (logFunc == null) throw new ArgumentNullException(nameof(logFunc));

            return logger.Log(log =>
            {
                log.With(LogProperties.Level, logLevel).MessageFunc(messageFunc);
                return log;
            });
        }

        #endregion

        #region Other

        public static T Return<T>(this ILogger logger, T obj)
        {
            return obj;
        }

        #endregion

        #region BeginScope	

        public static ILogScope BeginScope(this ILogger logger, out object correlationId)
        {
            var scope = LogScope.Push();
            scope.WithCorrelationId(out correlationId);
            return scope;
        }

        public static ILogScope BeginScope(this ILogger logger)
        {
            return logger.BeginScope(out _);
        }

        /// <summary>
        /// Gets log scopes ordered by depths ascending.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<ILogScope> Scopes(this ILogger logger)
        {
            return
                LogScope
                    .Current
                    .Flatten();
        }

        #endregion

        public static ILoggerTransaction BeginTransaction(this ILogger logger)
        {
            return new LoggerTransaction(logger);
        }
    }
}