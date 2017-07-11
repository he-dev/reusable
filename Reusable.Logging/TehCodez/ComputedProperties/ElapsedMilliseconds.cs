using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class ElapsedMilliseconds : Elapsed
    {
        protected override double Compute(TimeSpan elapsed) => elapsed.TotalMilliseconds;
    }
}