using System;
using System.Diagnostics;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public abstract class Elapsed : ComputedProperty
    {
        protected Elapsed() => Digits = 1;

        public int Digits { get; set; }

        public override object Compute(LogEntry logEntry)
        {
            var stopwatch = logEntry.GetValueOrDefault<Stopwatch>(nameof(Stopwatch));
            return stopwatch == null ? null : (object)Math.Round(Compute(stopwatch.Elapsed), Digits);
        }

        protected abstract double Compute(TimeSpan elapsed);
    }
}