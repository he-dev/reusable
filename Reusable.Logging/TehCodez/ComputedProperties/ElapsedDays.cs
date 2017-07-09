using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedDays : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalDays;
    }
}