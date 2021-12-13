using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Data;

public class LogScope : ILogScope
{
    public string Name { get; init; } = default!;

    public ILogger Logger { get; init; } = default!;

    public ILoggerNode First { get; init; } = default!;

    public Stack<(Exception Exception, ILogCaller Caller)> Exceptions { get; } = new();

    public object? WorkItem { get; init; }

    public LogCaller LogCaller { get; init; } = default!;

    // Helps to prevent enumerating nodes beyond the scope pipeline.
    private bool IsMainPipeline(ILoggerNode node)
    {
        return !ReferenceEquals(node, First) && node.Prev is ToggleScope;
    }

    public IEnumerator<ILogScope> GetEnumerator()
    {
        return AsyncScope<ILogScope>.Current.Enumerate().Select(s => s.Value).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        foreach (var node in First.EnumerateNext().TakeWhile(node => !IsMainPipeline(node)))
        {
            node.Dispose();
        }

        var exception =
            Exceptions.Any()
                ? Exceptions.Count > 1
                    ? new AggregateException(Exceptions.Select(t => t.Exception))
                    : Exceptions.Pop().Exception
                : default;

        Logger.Log
        (
            Telemetry
                .Collect
                .Application()
                .Task(Name)
                .Status(Helpers.GetFlowStatus(exception)) // todo: WorkItem was used here.
                .Level(Helpers.GetLogLevel(exception))
                .Exception(exception)
                .Then(x => x.Caller(LogCaller).Push(new MetaProperty.OverrideBuffer()))
        );

        AsyncScope<ILogScope>.Current?.Dispose();
    }

    private static class Helpers
    {
        public static FlowStatus GetFlowStatus(Exception? exception)
        {
            return exception switch
            {
                null => FlowStatus.Completed,
                OperationCanceledException _ => FlowStatus.Canceled,
                { } => FlowStatus.Faulted
            };
        }

        public static LogLevel GetLogLevel(Exception? exception)
        {
            return exception switch
            {
                null => LogLevel.Information,
                OperationCanceledException e => LogLevel.Warning,
                { } e => LogLevel.Error
            };
        }
    }
}

public static class StackExtensions
{
    public static void Push
    (
        this Stack<(Exception, ILogCaller)> stack,
        Exception exception,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        stack.Push((exception, new LogCaller(callerMemberName, callerLineNumber, callerFilePath)));
    }
}