using System;
using System.Diagnostics;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Middleware
{
    /// <summary>
    /// Adds 'Elapsed' milliseconds to the log.
    /// </summary>
    public class LoggerStopwatch : LoggerMiddleware, ILoggerScope<LoggerStopwatch.Scope, object>
    {
        private readonly string _propertyName;

        public LoggerStopwatch(string propertyName = nameof(Stopwatch.Elapsed)) : base(false)
        {
            _propertyName = propertyName;
        }

        public override bool IsActive => !(LoggerScope<Scope>.Current is null);

        #region IScope

        public Scope Push(object parameter)
        {
            return LoggerScope<Scope>.Push(new Scope()).Value;
        }

        #endregion

        public Func<TimeSpan, double> GetValue { get; set; } = ts => ts.TotalMilliseconds;

        protected override void InvokeCore(Log request)
        {
            request.SetItem((_propertyName, default), (long)GetValue(LoggerScope<Scope>.Current.Value.Elapsed));
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
}