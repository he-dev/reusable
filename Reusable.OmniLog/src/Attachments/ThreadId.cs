using System.Threading;
using Reusable.OmniLog.Abstractions;

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