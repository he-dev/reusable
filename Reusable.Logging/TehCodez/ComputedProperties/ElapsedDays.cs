using System;

namespace Reusable.Logging.ComputedProperties
{
    public class ElapsedDays : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalDays;
    }
}