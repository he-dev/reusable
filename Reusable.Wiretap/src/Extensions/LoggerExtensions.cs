﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Extensions
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        #region By log-evel

        public static void Trace
        (
            this ILogger logger,
            string message,
            Action<ILogEntry>? action = null,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(e => e.Level(LogLevel.Trace).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Debug
        (
            this ILogger logger,
            string message,
            Action<ILogEntry>? action = null,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(e => e.Level(LogLevel.Debug).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
        }
        
        public static void Warning
        (
            this ILogger logger,
            string message,
            Action<ILogEntry>? action = null,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(e => e.Level(LogLevel.Warning).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
        }
        
        public static void Information
        (
            this ILogger logger,
            string message,
            Action<ILogEntry>? action = null,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(e => e.Level(LogLevel.Information).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
        }
        
        public static void Error
        (
            this ILogger logger,
            string message,
            Action<ILogEntry>? action = null,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(e => e.Level(LogLevel.Error).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
        }
        
        public static void Fatal
        (
            this ILogger logger,
            string message,
            Action<ILogEntry>? action = null,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.Log(e => e.Level(LogLevel.Fatal).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
        }
        

        #endregion

        public static void Log(this ILogger logger, params object[] items)
        {
            logger.PushProperties(items.Where(x => x is {}));
            logger.Log(LogEntry.Empty());
        }

        public static void Log
        (
            this ILogger logger,
            Action<ILogEntry> action,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int? callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            logger.PushDelegate(action.Then(log =>
            {
                log.Push(new LoggableProperty.CallerMemberName(callerMemberName!));
                log.Push(new LoggableProperty.CallerLineNumber(callerLineNumber!));
                log.Push(new LoggableProperty.CallerFilePath(callerFilePath!));
            }));
            logger.Log(LogEntry.Empty());
        }

        #region HtmlConsole

        /// <summary>
        /// Writes to console without line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void Write(this ILogger console, IConsoleStyle style, params IHtmlConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(false, style, builders));
        }

        /// <summary>
        /// Writes to console line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void WriteLine(this ILogger console, IConsoleStyle style, params IHtmlConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(true, style, builders));
        }

        private static ILogEntry ConsoleTemplateBuilder(this ILogEntry logEntry, bool isParagraph, IConsoleStyle style, IEnumerable<IHtmlConsoleTemplateBuilder> builders)
        {
            return logEntry.Also(e => e.Push(new HtmlProperty.Message(new HtmlConsoleTemplateBuilder(isParagraph, style, builders))));
            //return logEntry.Push( Names.Properties.Message, new HtmlConsoleTemplateBuilder(isParagraph, style, builders), m => m.ProcessWith<Echo>().LogWith<HtmlConsoleRx>());
        }

        #endregion

        public static T Node<T>(this ILoggerNode node) where T : ILoggerNode
        {
            return node.EnumerateNext().OfType<T>().SingleOrThrow(onEmpty: () => DynamicException.Create($"{nameof(LoggerNode)}NotFound", $"There was no {typeof(T).ToPrettyString()}."));
        }

        public static T? NodeOrDefault<T>(this ILoggerNode node) where T : class, ILoggerNode
        {
            return node.EnumerateNext().OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets logger-node of the specified type.
        /// </summary>
        public static T Node<T>(this ILogger logger) where T : ILoggerNode
        {
            return ((ILoggerNode)logger).Node<T>();
        }
    }
}