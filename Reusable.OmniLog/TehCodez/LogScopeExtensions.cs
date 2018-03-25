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
        public static LogScope AttachElapsed(this LogScope scope)
        {
            return scope.With(new ElapsedMilliseconds(nameof(Elapsed)));
        }
    }
}