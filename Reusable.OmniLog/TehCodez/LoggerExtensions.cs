using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public delegate string MessageFunc();

    public static class LoggerExtensions
    {
        #region LogLevels

        public static ILogger Trace(this ILogger logger, string message, Func<Log, Log> logAction = null)
        {
            return logger.Log(LogLevel.Trace, message, null, logAction);
        }

        public static ILogger Debug(this ILogger logger, string message, Func<Log, Log> logAction = null)
        {
            return logger.Log(LogLevel.Debug, message, null, logAction);
        }

        public static ILogger Warning(this ILogger logger, string message, Func<Log, Log> logAction = null)
        {
            return logger.Log(LogLevel.Warning, message, null, logAction);
        }

        public static ILogger Information(this ILogger logger, string message, Func<Log, Log> logAction = null)
        {
            return logger.Log(LogLevel.Information, message, null, logAction);
        }

        public static ILogger Error(this ILogger logger, string message, Exception exception = null, Func<Log, Log> logAction = null)
        {
            return logger.Log(LogLevel.Error, message, exception, logAction);
        }

        public static ILogger Fatal(this ILogger logger, string message, Exception exception = null, Func<Log, Log> logAction = null)
        {
            return logger.Log(LogLevel.Fatal, message, exception, logAction);
        }

        private static ILogger Log(
            [NotNull] this ILogger logger,
            [NotNull] LogLevel logLevel,
            [CanBeNull] string message,
            [CanBeNull] Exception exception,
            [CanBeNull] Func<Log, Log> logAction
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));

            return logger.Log(logLevel, log =>
            {
                log.Message(message);
                log.Exception(exception);
            });
        }

        #endregion

        #region LogLevels lazy

        public static ILogger Trace(this ILogger logger, MessageFunc messageFunc, Func<Log, Log> logFunc = null)
        {
            return logger.Log(LogLevel.Trace, messageFunc, logFunc ?? (_ => _));
        }               

        public static ILogger Debug(this ILogger logger, MessageFunc messageFunc, Func<Log, Log> logFunc = null)
        {
            return logger.Log(LogLevel.Debug, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Information(this ILogger logger, MessageFunc messageFunc, Func<Log, Log> logFunc = null)
        {
            return logger.Log(LogLevel.Information, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Warning(this ILogger logger, MessageFunc messageFunc, Func<Log, Log> logFunc = null)
        {
            return logger.Log(LogLevel.Warning, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Error(this ILogger logger, MessageFunc messageFunc, Func<Log, Log> logFunc = null)
        {
            return logger.Log(LogLevel.Error, messageFunc, logFunc ?? (_ => _));
        }

        public static ILogger Fatal(this ILogger logger, MessageFunc messageFunc, Func<Log, Log> logFunc = null)
        {
            return logger.Log(LogLevel.Fatal, messageFunc, logFunc ?? (_ => _));
        }
        
        public static T TraceReturn<T>(this ILogger logger, T obj, Func<T, string> messageFunc, Func<Log, Log> logFunc = null)
        {
            logger.Log(LogLevel.Trace, () => messageFunc(obj), logFunc ?? (_ => _));
            return obj;
        }
        
        public static T DebugReturn<T>(this ILogger logger, T obj, Func<T, string> messageFunc, Func<Log, Log> logFunc = null)
        {
            logger.Log(LogLevel.Debug, () => messageFunc(obj), logFunc ?? (_ => _));
            return obj;
        }

        private static ILogger Log(
            [NotNull] this ILogger logger,
            [NotNull] LogLevel logLevel,
            [NotNull] MessageFunc messageFunc,
            [NotNull] Func<Log, Log> logFunc
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));
            if (messageFunc == null) throw new ArgumentNullException(nameof(messageFunc));
            if (logFunc == null) throw new ArgumentNullException(nameof(logFunc));

            return logger.Log(logLevel, log => log.MessageFunc(messageFunc));
        }

        #endregion

        #region Other

        public static T Return<T>(this ILogger logger, T obj)
        {
            return obj;
        }

        #endregion

        #region BeginScope	

        public static LogScope BeginScope(this ILogger logger, object state)
        {
            return logger.BeginScope(null, state, log => { });
        }

        public static LogScope BeginScope(this ILogger logger, Action<Log> logAction)
        {
            return logger.BeginScope(null, null, logAction);
        }

        public static LogScope BeginScope(this ILogger logger, SoftString name, object state)
        {
            return logger.BeginScope(name, state, log => { });
        }

        public static LogScope BeginScope(this ILogger logger, SoftString name, Action<Log> logAction)
        {
            return logger.BeginScope(name, null, logAction);
        }

        public static LogScope BeginScope(this ILogger logger, SoftString name, object state, Action<Log> logAction)
        {
            return LogScope.Push(name, Reflection.GetProperties(state), logAction);
        }

        #endregion
    }
}