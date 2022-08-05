using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Extensions;

[PublicAPI]
public static class LoggerExtensions
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

    #region UnitOfWork

    /// <summary>
    /// Initializes a new unit-of-work using the caller's name or a custom one.
    /// </summary>
    [MustUseReturnValue]
    public static UnitOfWork.Item BeginUnitOfWork(this ILogger logger, string? tag = default, object? id = default, [CallerMemberName] string? callerMemberName = default)
    {
        return logger.Node<UnitOfWork>().Push(logger, tag ?? callerMemberName!, id);
    }

    /// <summary>
    /// Gets the current unit-of-work.
    /// </summary>
    [MustUseReturnValue]
    public static UnitOfWork.Item UnitOfWork(this ILogger logger)
    {
        return Middleware.UnitOfWork.Current ?? throw new InvalidOperationException($"You can use this method only with an active {nameof(Middleware.UnitOfWork)} scope.");
    }

    #endregion
}