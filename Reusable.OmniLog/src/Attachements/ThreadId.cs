using System.Threading;

namespace Reusable.OmniLog.Attachements
{
    public class ThreadId : LogAttachement
    {
        public override object Compute(ILog log)
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}