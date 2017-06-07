using System;

namespace Reusable.Logging.ComputedProperties
{
    public class ElapsedMilliseconds : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalMilliseconds;
    }
}