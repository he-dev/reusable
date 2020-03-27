using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Data;
using Reusable.OmniLog.Nodes;
using CallSite = Reusable.OmniLog.Data.CallSite;
using Telemetry = Reusable.OmniLog.Extensions.Telemetry;

// ReSharper disable ExplicitCallerInfoArgument

namespace Reusable.OmniLog
{
    public class LoggerScope : ILoggerScope
    {
        public string Name { get; set; } = default!;

        public ILogger Logger { get; set; } = default!;

        public ILoggerNode First { get; set; } = default!;

        public Stack<(Exception Exception, Data.CallSite CallSite)> Exceptions { get; } = new Stack<(Exception, Data.CallSite)>();

        public object? WorkItem { get; set; }

        public CallSite CallSite { get; set; } = default!;

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
                    .Then(x => x.CallSite(CallSite).Priority(LogEntryPriority.High))
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
            this Stack<(Exception, Data.CallSite)> stack,
            Exception exception,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            stack.Push((exception, new Data.CallSite(callerMemberName, callerLineNumber, callerFilePath)));
        }
    }
}