using System.Threading;

namespace Reusable.Logging.ComputedProperties
{
    public class ThreadId : ComputedProperty
    {
        public override object Compute(LogEntry log) => Thread.CurrentThread.ManagedThreadId;
    }
}