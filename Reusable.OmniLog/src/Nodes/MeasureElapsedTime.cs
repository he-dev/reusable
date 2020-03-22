using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds 'Elapsed' time to the log.
    /// </summary>
    public class MeasureElapsedTime : LoggerNode
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public Func<TimeSpan, double> GetValue { get; set; } = ts => ts.TotalMilliseconds;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public override void Invoke(ILogEntry request)
        {
            request.Push(Names.Properties.Elapsed, (long)GetValue(Elapsed), LogProperty.Process.With<Echo>());
            InvokeNext(request);
        }

        public void Reset() => _stopwatch.Reset();
    }
}