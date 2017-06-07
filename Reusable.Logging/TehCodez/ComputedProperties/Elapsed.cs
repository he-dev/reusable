using System;
using System.Diagnostics;

namespace Reusable.Logging.ComputedProperties
{
    public abstract class Elapsed : ComputedProperty
    {
        protected Elapsed() => Digits = 1;

        public int Digits { get; set; }

        public override object Compute(LogEntry logEntry)
        {
            var stopwatch = logEntry.GetValue<Stopwatch>(nameof(Stopwatch));
            return stopwatch == null ? null : (object)Math.Round(ComputeCore(stopwatch.Elapsed), Digits);
        }

        protected abstract double ComputeCore(TimeSpan elapsed);
    }
}