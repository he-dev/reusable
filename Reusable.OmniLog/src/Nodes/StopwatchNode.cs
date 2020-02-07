using System;
using System.Diagnostics;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds 'Elapsed' milliseconds to the log.
    /// </summary>
    public class StopwatchNode : LoggerNode
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        protected override void invoke(ILogEntry request)
        {
            request.Add(LogProperty.Names.Elapsed, (long)GetValue(Elapsed), m => m.ProcessWith<EchoNode>());
            invokeNext(request);
        }

        public Func<TimeSpan, double> GetValue { get; set; } = ts => ts.TotalMilliseconds;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public void Reset() => _stopwatch.Reset();
    }

    public static class LoggerStopwatchHelper
    {
        /// <summary>
        /// Activates a new stopwatch and returns it.
        /// </summary>
        public static ILoggerScope UseStopwatch(this ILoggerScope scope) => scope.AddNode(new StopwatchNode());

        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static StopwatchNode? Stopwatch(this ScopeNode.FirstNode scope) => scope.EnumerateNext().OfType<StopwatchNode>().FirstOrDefault();
    }
}