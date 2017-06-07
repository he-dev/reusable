using System;

namespace Reusable.Logging.ComputedProperties
{
    public class ElapsedMinutes : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalMinutes;
    }
}