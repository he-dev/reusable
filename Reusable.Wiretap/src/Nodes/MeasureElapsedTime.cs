using System;
using System.Diagnostics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Utilities;

namespace Reusable.Wiretap.Nodes
{
    using static TimeSpanPrecision;

    /// <summary>
    /// This node adds 'Elapsed' time to the log.
    /// </summary>
    public class MeasureElapsedTime : LoggerNode
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public MeasureElapsedTime()
        {
            Precision = Milliseconds;
            GetValue = ts => ts.ToDouble(Precision);
        }

        public TimeSpanPrecision Precision { get; set; }

        public Func<TimeSpan, double> GetValue { get; set; }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public override void Invoke(ILogEntry entry)
        {
            entry.Push(new LoggableProperty.Elapsed((long)GetValue(Elapsed)));
            InvokeNext(entry);
        }

        public void Reset() => _stopwatch.Reset();
    }
}