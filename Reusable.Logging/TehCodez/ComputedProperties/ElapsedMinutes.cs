using System;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class ElapsedMinutes : Elapsed
    {
        protected override double Compute(TimeSpan elapsed) => elapsed.TotalMinutes;
    }
}