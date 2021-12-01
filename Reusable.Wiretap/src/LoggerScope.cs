using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Telemetry = Reusable.Wiretap.Extensions.Telemetry;

// ReSharper disable ExplicitCallerInfoArgument

namespace Reusable.Wiretap
{
    public class LoggerScope : ILoggerScope
    {
        public string Name { get; set; } = default!;

        public ILogger Logger { get; set; } = default!;

        public ILoggerNode First { get; set; } = default!;

        public Stack<(Exception Exception, ICaller Caller)> Exceptions { get; } = new Stack<(Exception, ICaller)>();

        public object? WorkItem { get; set; }

        public Caller Caller { get; set; } = default!;

        // Helps to prevent enumerating nodes beyond the scope pipeline.
        private bool IsMainPipeline(ILoggerNode node)
        {
            return !ReferenceEquals(node, First) && node.Prev is ToggleScope;
        }

        public IEnumerator<ILoggerScope> GetEnumerator()
        {
            return AsyncScope<ILoggerScope>.Current.Enumerate().Select(s => s.Value).GetEnumerator();
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
                    .Status(Helpers.GetFlowStatus(exception), WorkItem)
                    .Level(Helpers.GetLogLevel(exception))
                    .Exception(exception)
                    .Then(x => x.Caller(Caller).Priority(LogEntryPriority.High))
            );

            AsyncScope<ILoggerScope>.Current?.Dispose();
        }

        private static class Helpers
        {
            public static FlowStatus GetFlowStatus(Exception? exception)
            {
                return exception switch
                {
                    null => FlowStatus.Completed,
                    OperationCanceledException _ => FlowStatus.Canceled,
                    {} => FlowStatus.Faulted
                };
            }

            public static LogLevel GetLogLevel(Exception? exception)
            {
                return exception switch
                {
                    null => LogLevel.Information,
                    OperationCanceledException e => LogLevel.Warning,
                    {} e => LogLevel.Error
                };
            }
        }
    }

    public static class StackExtensions
    {
        public static void Push
        (
            this Stack<(Exception, ICaller)> stack,
            Exception exception,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            stack.Push((exception, new Caller(callerMemberName, callerLineNumber, callerFilePath)));
        }
    }
}