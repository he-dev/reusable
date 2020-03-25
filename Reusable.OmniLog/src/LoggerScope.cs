using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public class LoggerScope : ILoggerScope
    {
        public ILogger Logger { get; set; } = default!;

        public ILoggerNode First { get; set; } = default!;

        public Stack<Exception> Exceptions { get; } = new Stack<Exception>();

        internal Action<ILogger, Exception?> OnEndScope { get; set; } = default!;

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
                        ? new AggregateException(Exceptions)
                        : Exceptions.Pop()
                    : default;

            OnEndScope(Logger, exception);
            
            AsyncScope<ILoggerScope>.Current?.Dispose();
        }
    }
}