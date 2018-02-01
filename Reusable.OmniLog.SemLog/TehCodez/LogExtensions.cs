using System.Linq.Custom;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions
{
    public static class LogExtensions
    {
        /// <summary>
        /// Attaches transaction to each log.
        /// </summary>
        public static Log Transaction(this Log log, string name, object obj)
        {
            return log.With(name, obj);
        }

        /// <summary>
        /// Attaches elapsed-milliseconds to each log.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static Log Elapsed(this Log log)
        {
            log.Add(new ElapsedMilliseconds(nameof(Elapsed)).ToLogProperty());
            return log;
        }

        /// <summary>
        /// Attaches layer to each log.
        /// </summary>
        public static Log Layer(this Log log, Layer layer)
        {
            return log.With(nameof(Layer), layer);
        }
    }
}