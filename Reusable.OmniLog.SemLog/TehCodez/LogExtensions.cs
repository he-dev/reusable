using Reusable.Extensions;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemLog
{
    public static class LogExtensions
    {
        public static Log TransactionId(this Log log, object transactionId)
        {
            log.Property<string>(transactionId.ToString());
            return log;
        }

        public static Log WithElapsed(this Log log)
        {
            return log.Then(l => l.Add(new ElapsedMilliseconds("Elapsed").ToLogProperty()));
        }
    }
}