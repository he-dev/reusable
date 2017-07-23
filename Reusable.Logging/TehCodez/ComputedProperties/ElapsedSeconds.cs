using System;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class ElapsedSeconds : Elapsed
    {
        protected override double Compute(TimeSpan elapsed) => elapsed.TotalSeconds;
    }
}