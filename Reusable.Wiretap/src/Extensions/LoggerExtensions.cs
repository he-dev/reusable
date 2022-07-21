using System;
using System.Collections.Generic;
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
    public static void Log
    (
        this ILogger logger,
        Action<ILogEntry> action,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        logger.Log(LogEntry.Empty(), entry => entry.Also(action), callerMemberName, callerLineNumber, callerFilePath);
    }

    public static void Log
    (
        this ILogger logger,
        ILogEntry entry,
        Action<ILogEntry> action,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        logger.Log(
            entry
                .Also(action)
                .Push<IRegularProperty>(nameof(callerMemberName), callerMemberName!)
                .Push<IRegularProperty>(nameof(callerLineNumber), callerLineNumber!)
                .Push<IRegularProperty>(nameof(callerFilePath), callerFilePath!)
        );
    }

    #region Log-levels
    
    // public static void Trace
    // (
    //     this ILogger logger,
    //     string message,
    //     Action<ILogEntry>? action = null,
    //     [CallerMemberName] string? callerMemberName = null,
    //     [CallerLineNumber] int? callerLineNumber = 0,
    //     [CallerFilePath] string? callerFilePath = null
    // )
    // {
    //     logger.Log(e => e.Level(LogLevel.Trace).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
    // }
    //
    // public static void Debug
    // (
    //     this ILogger logger,
    //     string message,
    //     Action<ILogEntry>? action = null,
    //     [CallerMemberName] string? callerMemberName = null,
    //     [CallerLineNumber] int? callerLineNumber = 0,
    //     [CallerFilePath] string? callerFilePath = null
    // )
    // {
    //     logger.Log(e => e.Level(LogLevel.Debug).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
    // }
    //
    // public static void Warning
    // (
    //     this ILogger logger,
    //     string message,
    //     Action<ILogEntry>? action = null,
    //     [CallerMemberName] string? callerMemberName = null,
    //     [CallerLineNumber] int? callerLineNumber = 0,
    //     [CallerFilePath] string? callerFilePath = null
    // )
    // {
    //     logger.Log(e => e.Level(LogLevel.Warning).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
    // }
    //
    // public static void Information
    // (
    //     this ILogger logger,
    //     string message,
    //     Action<ILogEntry>? action = null,
    //     [CallerMemberName] string? callerMemberName = null,
    //     [CallerLineNumber] int? callerLineNumber = 0,
    //     [CallerFilePath] string? callerFilePath = null
    // )
    // {
    //     logger.Log(e => e.Level(LogLevel.Information).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
    // }
    //
    // public static void Error
    // (
    //     this ILogger logger,
    //     string message,
    //     Action<ILogEntry>? action = null,
    //     [CallerMemberName] string? callerMemberName = null,
    //     [CallerLineNumber] int? callerLineNumber = 0,
    //     [CallerFilePath] string? callerFilePath = null
    // )
    // {
    //     logger.Log(e => e.Level(LogLevel.Error).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
    // }
    //
    // public static void Fatal
    // (
    //     this ILogger logger,
    //     string message,
    //     Action<ILogEntry>? action = null,
    //     [CallerMemberName] string? callerMemberName = null,
    //     [CallerLineNumber] int? callerLineNumber = 0,
    //     [CallerFilePath] string? callerFilePath = null
    // )
    // {
    //     logger.Log(e => e.Level(LogLevel.Fatal).Message(message).Also(action), callerMemberName, callerLineNumber, callerFilePath);
    // }

    #endregion

    // public static void Log__(this ILogger logger, params object[] items)
    // {
    //     var entry = LogEntry.Empty();
    //     foreach (var (value, index) in items.Where(x => x is { }).Select((x, i) => (x, i)))
    //     {
    //         entry.Push<ITransientProperty>($"Item{index}", value);
    //     }
    //
    //     //logger.PushProperties(items.Where(x => x is {}));
    //     //logger.Log(LogEntry.Empty().Push(new DynamicProperty.Unknown(items)));
    //     logger.Log(entry);
    // }

    public static T? NodeOrDefault<T>(this ILoggerNode node) where T : class, ILoggerNode
    {
        return node.EnumerateNext().OfType<T>().FirstOrDefault();
    }
}