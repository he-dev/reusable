using System;
using System.Diagnostics;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.v2;

namespace Reusable.OmniLog.v2.Middleware
{
    /// <summary>
    /// Adds 'Elapsed' milliseconds to the log.
    /// </summary>
    public class LoggerStopwatch : LoggerMiddleware, ILoggerScope<LoggerStopwatchScope, object>
    {
        private readonly string _propertyName;

        public LoggerStopwatch(string propertyName = nameof(Stopwatch.Elapsed))
        {
            _propertyName = propertyName;
            IsActive = false;
        }

        public override bool IsActive => LoggerScope<LoggerStopwatchScope>.IsEmpty == false;

        #region IScope

        public LoggerStopwatchScope Push(object parameter)
        {
            return LoggerScope<LoggerStopwatchScope>.Push(new LoggerStopwatchScope());
        }

        #endregion

        public Func<TimeSpan, double> GetValue { get; set; } = ts => ts.TotalMilliseconds;

        protected override void InvokeCore(ILog request)
        {
            request[_propertyName] = GetValue(LoggerScope<LoggerStopwatchScope>.Peek().Elapsed);
            Next?.Invoke(request);
        }
    }

    public class LoggerStopwatchScope : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        public LoggerStopwatchScope()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public void Reset() => _stopwatch.Reset();

        public void Dispose() => LoggerScope<LoggerStopwatchScope>.Pop();
    }
}