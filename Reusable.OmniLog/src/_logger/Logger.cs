using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(ILogger))]

namespace Reusable.OmniLog
{
    public delegate bool CanLogCallback(ILog log);

    public class Logger : ILogger, IObservable<ILog>
    {
        private readonly TransformCallback _initialize;
        private readonly TransformCallback _attach;
        public static readonly IEnumerable<ILogLevel> LogLevels;

        private bool _disposed;

        //private readonly SoftString _name;
        private readonly CanLogCallback _canLog;
        private readonly Action _dispose;
        private readonly IList<IObserver<ILog>> _observers;

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

        internal Logger(TransformCallback initialize, TransformCallback attach, CanLogCallback canLog, Action dispose)
        {
            _initialize = initialize;
            _attach = attach;
            _canLog = canLog;
            _dispose = dispose;
            _observers = new List<IObserver<ILog>>();
        }

        public static ILogger Empty => new Logger(OmniLog.Log.EmptyTransform, OmniLog.Log.EmptyTransform, _ => true, () => { });

        #region ILogger

        public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default)
        {
            return Log((customizeResult ?? OmniLog.Log.EmptyTransform)(_attach(populate(_initialize(OmniLog.Log.Empty)))));
        }

        public ILogger Log(ILog log)
        {
            if (log.Any() && _canLog(log))
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(log);
                }
            }

            return this;
        }

        #endregion

        #region IObservable<Log>

        public IDisposable Subscribe(IObserver<ILog> observer)
        {
            _observers.Add(observer);
            return Disposable.Create(() => _observers.Remove(observer));
        }

        #endregion

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _observers.Clear();
            _dispose();
            _disposed = true;
        }
    }

    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory) : this(loggerFactory.CreateLogger(typeof(T).ToPrettyString())) { }

        private Logger(ILogger logger)
        {
            _logger = logger;
        }

        public static ILogger<T> Null => new Logger<T>(Logger.Empty);

        public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default) => _logger.Log(populate);

        public ILogger Log(ILog log) => _logger.Log(log);

        public void Dispose() => _logger.Dispose();
    }


    
}