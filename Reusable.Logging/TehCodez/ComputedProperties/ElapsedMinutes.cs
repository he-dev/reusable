using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedMinutes : Elapsed
    {
        protected override double Compute(TimeSpan elapsed) => elapsed.TotalMinutes;
    }
}