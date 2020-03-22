using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds 'Elapsed' time to the log.
    /// </summary>
    public class StopwatchNode : LoggerNode
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public Func<TimeSpan, double> GetValue { get; set; } = ts => ts.TotalMilliseconds;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public override void Invoke(ILogEntry request)
        {
            request.Push(Names.Default.Elapsed, (long)GetValue(Elapsed), LogProperty.Process.With<EchoNode>());
            InvokeNext(request);
        }

        public void Reset() => _stopwatch.Reset();
    }

    public static class LoggerStopwatchHelper
    {
        

        
    }
}