using System;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Extensions;

[PublicAPI]
public static partial class LoggerExtensions
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
        //logger.PushProperties(items.Where(x => x is {}));
        logger.Log(LogEntry.Empty().Push(new GuessableProperty.Unknown(items)));
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
        logger.Log(
            LogEntry
                .Empty()
                .Push(new CallableProperty.EntryAction(action))
                .Push(new LoggableProperty.CallerMemberName(callerMemberName!))
                .Push(new LoggableProperty.CallerLineNumber(callerLineNumber!))
                .Push(new LoggableProperty.CallerFilePath(callerFilePath!)));
    }

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