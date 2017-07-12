using System.Threading;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class ThreadId : ComputedProperty
    {
        public override object Compute(LogEntry log) => Thread.CurrentThread.ManagedThreadId;
    }
}