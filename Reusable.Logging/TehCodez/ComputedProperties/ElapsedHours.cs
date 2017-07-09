using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedHours : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalHours;
    }
}