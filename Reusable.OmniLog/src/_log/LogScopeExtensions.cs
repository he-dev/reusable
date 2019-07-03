using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
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
            var attachment = new ElapsedMilliseconds(nameof(Elapsed));
            return scope.SetItem(attachment.Name, attachment);
        }

        public static ILogScope CorrelationId(this ILogScope scope, object correlationId)
        {
            return scope.SetItem(nameof(CorrelationId), correlationId);
        }

        public static ILogScope CorrelationId(this ILogScope scope, out object correlationId)
        {
            return scope.CorrelationId(correlationId = LogScope.NewCorrelationId());
        }

        [Obsolete("Use WithCorrelationHandle and log this as Meta.")]
        public static ILogScope CorrelationContext(this ILogScope scope, object correlationContext)
        {
            return scope.SetItem(nameof(CorrelationContext), correlationContext);
        }

        public static T CorrelationId<T>(this ILogScope scope)
        {
            return scope.GetItemOrDefault<T>(nameof(CorrelationId));
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
        public static ILogScope CorrelationHandle(this ILogScope scope, [NotNull] object correlationHandle)
        {
            if (correlationHandle == null) throw new ArgumentNullException(nameof(correlationHandle));

            return scope.SetItem(nameof(CorrelationHandle), correlationHandle);
        }
        
        public static ILogScope Routine(this ILogScope scope, string identifier)
        {
            return scope.SetItem(nameof(Routine), identifier);
        }
        
        public static ILogScope CurrentRoutine(this ILogScope scope, [CallerMemberName] string identifier = null)
        {
            return scope.Routine(identifier);
        }
    }
}