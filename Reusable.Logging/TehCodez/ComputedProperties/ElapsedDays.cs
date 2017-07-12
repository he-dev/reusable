using System;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class ElapsedDays : Elapsed
    {
        protected override double Compute(TimeSpan elapsed) => elapsed.TotalDays;
    }
}