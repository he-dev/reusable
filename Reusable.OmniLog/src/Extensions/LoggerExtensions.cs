using System;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx.ConsoleRenderers;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Extensions
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
            return logEntry.SetItem(LogEntry.BasicPropertyNames.Message, nameof(HtmlConsoleTemplateBuilder), htmlConsoleTemplateBuilder);
        }

        #endregion

        public static T Node<T>(this ILogger logger) where T : LoggerNode
        {
            return
                logger
                    .Node
                    .Enumerate(m => m.Next)
                    .OfType<T>()
                    .SingleOrThrow(onEmpty: () => DynamicException.Create($"{nameof(LoggerNode)}NotFound", $"There was no {typeof(T).ToPrettyString()}."));
        }
    }
}