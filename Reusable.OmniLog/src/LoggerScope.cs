using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public class LoggerScope : ILoggerScope
    {
        public ILogger Logger { get; set; } = default!;

        public ILoggerNode First { get; set; } = default!;

        public Stack<(Exception Exception, Data.CallSite CallSite)> Exceptions { get; } = new Stack<(Exception, Data.CallSite)>();

        internal Action<ILogger, Exception?, Data.CallSite?> OnEndScope { get; set; } = default!;

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

            var callSite = Exceptions.Select(t => t.CallSite).LastOrDefault();

            var exception =
                Exceptions.Any()
                    ? Exceptions.Count > 1
                        ? new AggregateException(Exceptions.Select(t => t.Exception))
                        : Exceptions.Pop().Exception
                    : default;

            OnEndScope(Logger, exception, callSite);

            AsyncScope<ILoggerScope>.Current?.Dispose();
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