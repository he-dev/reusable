using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes, Target = typeof(ILogger))]

namespace Reusable.OmniLog
{
    public class Logger : ILogger
    {
        private readonly Func<ILog, ILog> _initialize;
        private readonly Func<ILog, ILog> _attach;
        private readonly IObserver<ILog> _observer;
        public static readonly IEnumerable<ILogLevel> LogLevels;

        static Logger()
        {
            LogLevels = new[]
            {
                LogLevel.Trace,
                LogLevel.Debug,
                LogLevel.Information,
                LogLevel.Warning,
                LogLevel.Error,
                LogLevel.Fatal,
            };
        }

        internal Logger(Func<ILog, ILog> initialize, Func<ILog, ILog> attach, IObserver<ILog> observer)
        {
            _initialize = initialize;
            _attach = attach;
            _observer = observer;
        }

        public static ILogger Empty => new Logger(Functional.Echo, Functional.Echo, Observer.Create<ILog>(_ => { }));

        #region ILogger

        public ILogger Log(Func<ILog, ILog> request, Func<ILog, ILog> response = default)
        {
            return
                _initialize(OmniLog.Log.Empty)
                    .Pipe(request)
                    .Pipe(_attach)
                    .Pipe(response ?? Functional.Echo)
                    .Pipe(Log);
        }

        public ILogger Log(ILog log)
        {
            _observer.OnNext(log);
            return this;
        }

        #endregion
    }

    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory) : this(loggerFactory.CreateLogger(typeof(T).ToPrettyString())) { }

        private Logger(ILogger logger)
        {
            _logger = logger;
        }

        public static ILogger<T> Empty => new Logger<T>(Logger.Empty);

        public ILogger Log(Func<ILog, ILog> request, Func<ILog, ILog> response = default) => _logger.Log(request);

        public ILogger Log(ILog log) => _logger.Log(log);
    }
}