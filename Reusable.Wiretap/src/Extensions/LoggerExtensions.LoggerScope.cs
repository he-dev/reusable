using System;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Extensions;

public static partial class LoggerExtensions
{
    /// <summary>
    /// Creates a new scope that is open until disposed.
    /// </summary>
    [MustUseReturnValue]
    public static ILoggerScope BeginScope(this ILogger logger, string? name = default)
    {
        return logger.Node<ActivateScope>().Push().Also(() => name is { }, scope => scope.WithName(name!));
    }

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public static ILoggerScope Scope(this ILogger logger)
    {
        return logger.Node<ActivateScope>().Current;
    }

    public static ILoggerScope Exception(this ILoggerScope scope, Exception exception)
    {
        return scope.Also(s => s.Items[nameof(Exception)] = exception);
    }
}