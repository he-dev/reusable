using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Adds 'Elapsed' milliseconds to the log.
    /// </summary>
    public class StopwatchNode : LoggerNode, ILoggerScope<StopwatchNode.Scope, object>
    {
        public StopwatchNode() : base(false) { }

        public override bool Enabled => !(LoggerScope<Scope>.Current is null);

        #region IScope

        public Scope Push(object parameter)
        {
            return LoggerScope<Scope>.Push(new Scope()).Value;
        }

        #endregion

        public Func<TimeSpan, double> GetValue { get; set; } = ts => ts.TotalMilliseconds;

        protected override void InvokeCore(LogEntry request)
        {
            request.SetItem(LogEntry.Names.Elapsed, default, (long)GetValue(LoggerScope<Scope>.Current.Value.Elapsed));
            Next?.Invoke(request);
        }

        public class Scope : IDisposable
        {
            private readonly Stopwatch _stopwatch;

            public Scope()
            {
                _stopwatch = Stopwatch.StartNew();
            }

            public TimeSpan Elapsed => _stopwatch.Elapsed;

            public void Reset() => _stopwatch.Reset();

            public void Dispose() => LoggerScope<Scope>.Current.Dispose();
        }
    }

    public static class LoggerStopwatchHelper
    {
        public static StopwatchNode.Scope UseStopwatch(this ILogger logger)
        {
            return
                logger
                    .Node<StopwatchNode>()
                    .Push(default);
        }
    }
}