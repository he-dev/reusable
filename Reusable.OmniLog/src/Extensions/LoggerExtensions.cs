using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx.ConsoleRenderers;
using Reusable.OmniLog.Utilities;

// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicitly set it via overloads.

namespace Reusable.OmniLog
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        #region LogLevels

        public static void Trace(this ILogger logger, string message, AlterLogEntryCallback? alter = null)
        {
            logger.Log(LogLevel.Trace, message, null, alter);
        }

        public static void Debug(this ILogger logger, string message, AlterLogEntryCallback? alter = null)
        {
            logger.Log(LogLevel.Debug, message, null, alter);
        }

        public static void Warning(this ILogger logger, string message, AlterLogEntryCallback? alter = null)
        {
            logger.Log(LogLevel.Warning, message, null, alter);
        }

        public static void Information(this ILogger logger, string message, AlterLogEntryCallback? alter = null)
        {
            logger.Log(LogLevel.Information, message, null, alter);
        }

        public static void Error(this ILogger logger, string message, Exception? exception = null, AlterLogEntryCallback? alter = null)
        {
            logger.Log(LogLevel.Error, message, exception, alter);
        }

        public static void Fatal(this ILogger logger, string message, Exception? exception = null, AlterLogEntryCallback? alter = null)
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
            this ILogger logger,
            Option<LogLevel> level,
            string? message,
            Exception? exception,
            AlterLogEntryCallback? alter
        )
        {
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

        #region HtmlConsole

        /// <summary>
        /// Writes to console without line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void Write(this ILogger console, ConsoleStyle style, params HtmlConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(new CompositeHtmlConsoleTemplateBuilder
            {
                IsParagraph = false,
                Builders = builders,
                Style = style
            }));
        }

        /// <summary>
        /// Writes to console line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void WriteLine(this ILogger console, ConsoleStyle style, params HtmlConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(new CompositeHtmlConsoleTemplateBuilder
            {
                IsParagraph = true,
                Builders = builders,
                Style = style
            }));
        }

        private static LogEntry ConsoleTemplateBuilder(this LogEntry logEntry, HtmlConsoleTemplateBuilder htmlConsoleTemplateBuilder)
        {
            return logEntry.Add<Build>(LogEntry.Names.Message, htmlConsoleTemplateBuilder);
            //return logEntry.Add<Meta>(LogEntry.Names.Message, nameof(HtmlConsoleTemplateBuilder), htmlConsoleTemplateBuilder);
        }

        #endregion

        /// <summary>
        /// Gets logger-node of the specified type.
        /// </summary>
        public static T Node<T>(this ILogger logger) where T : ILoggerNode
        {
            return
                logger
                    //.Enumerate(m => m.Next)
                    .EnumerateNext()
                    .OfType<T>()
                    .SingleOrThrow(onEmpty: () => DynamicException.Create($"{nameof(LoggerNode)}NotFound", $"There was no {typeof(T).ToPrettyString()}."));
        }

        #region LoggerExtensions for LogEntryBuilder

        // We use context as the name and not abstractionContext because it otherwise interferes with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            AlterLogEntryCallback? alter = null,
            // These properties are for free so let's just log them too.
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(log =>
            {
                log.Add<Copy>(context.Name, context);
                log.Add<Log>(LogEntry.Names.CallerMemberName, callerMemberName);
                log.Add<Log>(LogEntry.Names.CallerLineNumber, callerLineNumber);
                log.Add<Log>(LogEntry.Names.CallerFilePath, Path.GetFileName(callerFilePath));
                alter?.Invoke(log);
            });
        }

        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            string message,
            Exception exception,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Message(message).Exception(exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            string message,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Message(message),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log<T>
        (
            this ILogger logger,
            ILogEntryBuilder<T> context,
            Exception exception,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Exception(exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        #endregion
    }
}