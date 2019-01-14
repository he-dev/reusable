using System.Threading;

namespace Reusable.OmniLog.Attachments
{
    public class ThreadId : LogAttachment
    {
        public override object Compute(ILog log)
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}