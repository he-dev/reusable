using System;
using System.Collections.Generic;
using Reusable.OmniLog.Attachements;

namespace Reusable.OmniLog
{
    public static class LogScopeExtensions
    {
        public static IEnumerable<LogScope> Flatten(this LogScope scope)
        {
            var current = scope;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        /// <summary>
        /// Attaches elapsed-milliseconds to each log.
        /// </summary>
        public static ILogScope AttachElapsed(this ILogScope scope)
        {
            return scope.With(new ElapsedMilliseconds(nameof(Elapsed)));
        }

        public static ILogScope WithCorrelationId(this ILogScope scope, object correlationId)
        {
            return scope.With("CorrelationId", correlationId);
        }

        public static ILogScope WithCorrelationId(this ILogScope scope)
        {
            return scope.WithCorrelationId(Guid.NewGuid().ToString("N"));
        }

        public static ILogScope WithCorrelationContext(this ILogScope scope, object correlationContext)
        {
            return scope.With("CorrelationContext", correlationContext);
        }

        public static T CorrelationId<T>(this ILogScope scope)
        {
            return scope.Property<T>();
        }
    }
}