using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedSeconds : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalSeconds;
    }
}