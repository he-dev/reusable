using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Extensions;

public static partial class LoggerExtensions
{
    /// <summary>
    /// Initializes a new unit-of-work using the caller's name or a custom one.
    /// </summary>
    [MustUseReturnValue]
    public static UnitOfWork.Item BeginUnitOfWork(this ILogger logger, string? name = default, [CallerMemberName] string? callerMemberName = default)
    {
        return logger.Node<UnitOfWork>().Push(logger, name ?? callerMemberName!);
    }

    /// <summary>
    /// Gets the current unit-of-work.
    /// </summary>
    [MustUseReturnValue]
    public static UnitOfWork.Item UnitOfWork(this ILogger logger)
    {
        return Middleware.UnitOfWork.Current ?? throw new InvalidOperationException($"You can use this method only with an active {nameof(Middleware.UnitOfWork)} scope.");
    }
}