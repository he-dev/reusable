using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;

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
            request.Add(Names.Default.Elapsed, (long)GetValue(Elapsed), LogProperty.Process.With<EchoNode>());
            InvokeNext(request);
        }

        public void Reset() => _stopwatch.Reset();
    }

    public static class LoggerStopwatchHelper
    {
        [Obsolete("Don't use this API. It's been replaced by the BranchNode. It's empty and provided for backward compatibility.")]
        /// <summary>
        /// Activates a new stopwatch and returns it.
        /// </summary>
        public static ILoggerScope UseStopwatch(this ILoggerScope scope)
        {
            //return scope.Append(new StopwatchNode());
            return scope;
        }

        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static StopwatchNode Stopwatch(this ILoggerNode logger) => logger.Node<StopwatchNode>();
    }
}