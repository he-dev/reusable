using System.Linq.Custom;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions
{
    public static class LogExtensions
    {
        /// <summary>
        /// Attaches elapsed-milliseconds to each log.
        /// </summary>
        public static Log Elapsed(this Log log)
        {
            var elapsed = new ElapsedMilliseconds(nameof(Elapsed));
            return log.With(elapsed.Name, elapsed);
        }
    }
}