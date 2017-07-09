using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedMilliseconds : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalMilliseconds;
    }
}