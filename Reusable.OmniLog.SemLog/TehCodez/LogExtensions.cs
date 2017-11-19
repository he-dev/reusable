using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemLog
{
    public static class LogExtensions
    {
        public static Log Transaction(this Log log, object transaction)
        {
            log.Property<string>(transaction.ToString());
            return log;
        }

        public static Log Elapsed(this Log log)
        {
            log.Add(new ElapsedMilliseconds("Elapsed").ToLogProperty());
            return log;
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