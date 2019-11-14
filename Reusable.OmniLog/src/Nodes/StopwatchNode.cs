using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds 'Elapsed' milliseconds to the log.
    /// </summary>
    public class StopwatchNode : LoggerNode
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        protected override void invoke(LogEntry request)
        {
            request.Add<Log>(LogEntry.Names.Elapsed, (long)GetValue(Elapsed));
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
        public static ILoggerScope UseStopwatch(this ILoggerScope scope) => scope.AddMiddleware(new StopwatchNode());

        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static StopwatchNode Stopwatch(this ILogger logger) => logger.Node<StopwatchNode>();
    }
}