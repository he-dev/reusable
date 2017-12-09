using System.Collections.Generic;
using System.Linq;
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
        /// <param name="log"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static Log Transaction(this Log log, object transaction)
        {
            log.Property<string>(transaction.ToString());
            return log;
        }

        /// <summary>
        /// Attaches elapsed-milliseconds to each log.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static Log Elapsed(this Log log)
        {
            log.Add(new ElapsedMilliseconds("Elapsed").ToLogProperty());
            return log;
        }

        /// <summary>
        /// Attaches layer to each log.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Log Layer(this Log log, Layer layer)
        {
            return log.With(nameof(Layer), layer);
        }
    }

    public class TransactionMerge : ILogScopeMerge
    {
        public SoftString Name => "Transaction";

        public KeyValuePair<SoftString, object> Merge(IEnumerable<KeyValuePair<SoftString, object>> items)
        {
            return new KeyValuePair<SoftString, object>(items.First().Key, items.Select(i => i.Value.ToString()).Reverse().Join("/"));
        }
    }
}