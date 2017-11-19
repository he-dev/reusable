using System.Threading;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class ThreadId : LogAttachement
    {
        public override object Compute(Log log)
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}