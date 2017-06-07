using System;

namespace Reusable.Logging.ComputedProperties
{
    public class ElapsedHours : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalHours;
    }
}