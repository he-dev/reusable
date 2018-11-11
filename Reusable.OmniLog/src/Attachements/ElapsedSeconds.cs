using System.Diagnostics;

namespace Reusable.OmniLog.Attachements
{
    public class ElapsedSeconds : Elapsed
    {
        public override object Compute(ILog log)
        {
            return log.Property<Stopwatch>(null, nameof(Stopwatch)).Elapsed.TotalSeconds;
        }
    }
}