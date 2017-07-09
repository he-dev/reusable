using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedMinutes : Elapsed
    {
        protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalMinutes;
    }
}