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

        public static ILogger Trace(this ILogger logger, string message, TransformCallback populate = null)
        {
            return logger.Log(LogLevel.Trace, message, null, populate);
        }

        public static ILogger Debug(this ILogger logger, string message, TransformCallback populate = null)
        {
            return logger.Log(LogLevel.Debug, message, null, populate);
        }

        public static ILogger Warning(this ILogger logger, string message, TransformCallback populate = null)
        {
            return logger.Log(LogLevel.Warning, message, null, populate);
        }

        public static ILogger Information(this ILogger logger, string message, TransformCallback populate = null)
        {
            return logger.Log(LogLevel.Information, message, null, populate);
        }

        public static ILogger Error(this ILogger logger, string message, Exception exception = null, TransformCallback populate = null)
        {
            return logger.Log(LogLevel.Error, message, exception, populate);
        }

        public static ILogger Fatal(this ILogger logger, string message, Exception exception = null, TransformCallback populate = null)
        {
            return logger.Log(LogLevel.Fatal, message, exception, populate);
        }

        private static ILogger Log
        (
            [NotNull] this ILogger logger,
            [NotNull] LogLevel logLevel,
            [CanBeNull] string message,
            [CanBeNull] Exception exception,
            [CanBeNull] TransformCallback populate
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));

            return logger.Log(log => log
                .SetItem(LogPropertyNames.Level, logLevel)
                .SetItem(LogPropertyNames.Message, message)
                .SetItem(LogPropertyNames.Exception, exception)
                .Transform(populate ?? OmniLog.Log.EmptyTransform));
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
            scope.CorrelationId(out correlationId);
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