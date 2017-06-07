using System;

namespace Reusable.Logging.ComputedProperties
{
    public class ElapsedSeconds : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalSeconds;
    }
}