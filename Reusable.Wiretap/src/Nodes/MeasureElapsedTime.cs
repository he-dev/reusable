using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Data;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
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

        public override void Invoke(ILogEntry request)
        {
            request.Push(Names.Properties.Elapsed, (long)GetValue(Elapsed), LogProperty.Process.With<Echo>());
            InvokeNext(request);
        }

        public void Reset() => _stopwatch.Reset();
    }
}