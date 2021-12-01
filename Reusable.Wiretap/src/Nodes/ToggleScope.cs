using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

// ReSharper disable ExplicitCallerInfoArgument - this is fine because it needs to be overriden

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node turns logger-scope on or off. By default it logs BeginScope and EndScope entries for each scope.
/// </summary>
public class ToggleScope : LoggerNode
{
    public override bool Enabled => AsyncScope<ILoggerScope>.Any;

    public List<Func<ILoggerNode>> ScopeFactories { get; set; } = new();

    public ILoggerScope Current => AsyncScope<ILoggerScope>.Current?.Value ?? throw new InvalidOperationException($"Cannot use {nameof(Current)} when {nameof(ToggleScope)} is disabled. Create one with Logger.BeginScope() first.");

    public ILoggerScope Push(ILogger logger, string name, object? workItem, Caller caller)
    {
        try
        {
            var scope = new LoggerScope
            {
                Name = name,
                Logger = logger,
                WorkItem = workItem,
                Caller = caller,
                First = CreatePipeline(this, ScopeFactories),
            };
            return AsyncScope<ILoggerScope>.Push(scope).Value;
        }
        finally
        {
            if (Next?.First() is { } first)
            {
                //OnBeginScope(first.Node<Logger>(), name, callSite);
            }
        }
    }

    public override void Invoke(ILogEntry entry)
    {
        // Does not call InvokeNext because it routes the request over the scope which is connected to the next node.
        Current.First.Invoke(entry.Push(new MetaProperty.Scope(Current)));
    }

    private static ILoggerNode CreatePipeline(ILoggerNode main, IEnumerable<Func<ILoggerNode>> branch)
    {
        var last = branch.Select(f => f()).Join();
        last.Next = main.Next;
        var first = last.First();
        first.Prev = main;

        return first;
    }
}

public static class ToggleScopeHelper
{
    /// <summary>
    /// Creates a new scope that is open until disposed.
    /// </summary>
    [MustUseReturnValue]
    public static ILoggerScope BeginScope
    (
        this ILogger logger,
        string name,
        object? workItem = default,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return logger.Node<ToggleScope>().Push(logger, name, workItem, new Caller(callerMemberName, callerLineNumber, callerFilePath));
    }
        
    [MustUseReturnValue]
    public static ILoggerScope BeginScope<T>
    (
        this ILogger logger,
        object? workItem = default,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return logger.Node<ToggleScope>().Push(logger, typeof(T).ToPrettyString(), workItem, new Caller(callerMemberName, callerLineNumber, callerFilePath));
    }
        
    [MustUseReturnValue]
    public static ILoggerScope BeginScope<T>
    (
        this ILogger logger,
        T instance,
        object? workItem = default,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return logger.Node<ToggleScope>().Push(logger, typeof(T).ToPrettyString(), workItem, new Caller(callerMemberName, callerLineNumber, callerFilePath));
    }
        
        

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public static ILoggerScope Scope(this ILogger logger)
    {
        return logger.Node<ToggleScope>().Current;
    }
}