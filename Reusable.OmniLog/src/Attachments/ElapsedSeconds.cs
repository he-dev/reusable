using System.Diagnostics;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Attachments
{
    public class ElapsedSeconds : Elapsed
    {
        public override object Compute(ILog log)
        {
            return log.GetItemOrDefault<Stopwatch>(nameof(Stopwatch)).Elapsed.TotalSeconds;
        }
    }
}