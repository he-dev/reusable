using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Middleware;
using Reusable.OmniLog.v2;

namespace Reusable.OmniLog
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        #region LogLevels

        public static void Trace(this ILogger logger, string message, AlterLogEntryCallback alter = null)
        {
             logger.Log(LogLevel.Trace, message, null, alter);
        }

        public static void Debug(this ILogger logger, string message, AlterLogEntryCallback alter = null)
        {
             logger.Log(LogLevel.Debug, message, null, alter);
        }

        public static void Warning(this ILogger logger, string message, AlterLogEntryCallback alter = null)
        {
             logger.Log(LogLevel.Warning, message, null, alter);
        }

        public static void Information(this ILogger logger, string message, AlterLogEntryCallback alter = null)
        {
             logger.Log(LogLevel.Information, message, null, alter);
        }

        public static void Error(this ILogger logger, string message, Exception exception = null, AlterLogEntryCallback alter = null)
        {
             logger.Log(LogLevel.Error, message, exception, alter);
        }

        public static void Fatal(this ILogger logger, string message, Exception exception = null, AlterLogEntryCallback alter = null)
        {
             logger.Log(LogLevel.Fatal, message, exception, alter);
        }
        
        public static void Log(this ILogger logger, AlterLogEntryCallback alter)
        {
            logger.UseLambda(alter);
            logger.Log(new LogEntry());
        }

        private static void Log
        (
            [NotNull] this ILogger logger,
            [NotNull] LogLevel level,
            [CanBeNull] string message,
            [CanBeNull] Exception exception,
            [CanBeNull] AlterLogEntryCallback alter
        )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (level == null) throw new ArgumentNullException(nameof(level));

            logger.Log(log =>
            {
                log.Level(level);
                log.Message(message);
                log.Exception(exception);
                alter?.Invoke(log);
            });
        }

        #endregion

        #region Other

        public static T Return<T>(this ILogger logger, T obj)
        {
            return obj;
        }

        #endregion
    }
}