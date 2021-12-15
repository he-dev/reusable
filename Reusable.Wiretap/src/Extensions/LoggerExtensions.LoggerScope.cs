using System;
using JetBrains.Annotations;
using Reusable.Extensions;
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
    public static ILoggerScope BeginScope(this ILogger logger, string name)
    {
        return logger.Node<ToggleScope>().Push(logger, name);
    }

    [MustUseReturnValue]
    public static ILoggerScope BeginScope<T>(this ILogger logger)
    {
        return logger.Node<ToggleScope>().Push(logger, typeof(T).ToPrettyString());
    }

    [MustUseReturnValue]
    public static ILoggerScope BeginScope<T>(this ILogger logger, T instance)
    {
        return logger.Node<ToggleScope>().Push(logger, typeof(T).ToPrettyString());
    }


    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public static ILoggerScope Scope(this ILogger logger)
    {
        return logger.Node<ToggleScope>().Current;
    }
    
    public static ILoggerScope Exception(this ILoggerScope scope, Exception exception)
    {
        return scope.Also(s => s.Items[nameof(Exception)] = exception);
    }
    
    public static ILoggerScope Cancelled(this ILoggerScope scope)
    {
        return scope.Also(s => s.Items["Execution"] = nameof(TelemetryCategories.Cancelled));
    }
}