using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.OmniLog.Attachments;

namespace Reusable.OmniLog
{
    [PublicAPI]
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
            return scope.With(nameof(WithCorrelationId), correlationId);
        }

        // public static ILogScope WithCorrelationId(this ILogScope scope)
        // {
        //     return scope.WithCorrelationId(LogScope.NewCorrelationId());
        // }

        public static ILogScope WithCorrelationId(this ILogScope scope, out object correlationId)
        {
            return scope.WithCorrelationId(correlationId = LogScope.NewCorrelationId());
        }

        public static ILogScope WithCorrelationContext(this ILogScope scope, object correlationContext)
        {
            return scope.With(nameof(WithCorrelationContext), correlationContext);
        }

        public static T CorrelationId<T>(this ILogScope scope)
        {
            return scope.Property<T>();
        }

        [NotNull]
        [ItemNotNull]
        public static IEnumerable<T> CorrelationIds<T>(this IEnumerable<ILogScope> scopes)
        {
            return
                from scope in scopes
                select scope.CorrelationId<T>();
        }
        
        [NotNull]
        public static ILogScope WithCorrelationHandle(this ILogScope scope, [NotNull] object correlationHandle)
        {
            if (correlationHandle == null) throw new ArgumentNullException(nameof(correlationHandle));
            
            return scope.With(nameof(WithCorrelationHandle), correlationHandle);
        }
    }
}