using System.Diagnostics;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class ElapsedSeconds : Elapsed
    {
        public override object Compute(Log log)
        {
            return log.Property<Stopwatch>(null, nameof(Stopwatch)).Elapsed.TotalSeconds;
        }
    }
}