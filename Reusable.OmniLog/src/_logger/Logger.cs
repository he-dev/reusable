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

    public class Logger : ILogger, IObservable<ILog> //, IEquatable<ILogger>
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

//        internal Logger([NotNull] SoftString name, [NotNull] CanLogCallback canLog, Action dispose)
//        {
//            _name = name ?? throw new ArgumentNullException(nameof(name));
//            _canLog = canLog;
//            _dispose = dispose;
//
//            _observers = new HashSet<IObserver<ILog>>();
//        }

        internal Logger(TransformCallback initialize, TransformCallback attach, CanLogCallback canLog, Action dispose)
        {
            _initialize = initialize;
            _attach = attach;
            _canLog = canLog;
            _dispose = dispose;
            _observers = new List<IObserver<ILog>>();
        }

        //private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder => { builder.DisplayValue(x => _name); });

        #region ILogger

        //public SoftString Name => _name;

        public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default)
        {
            var log = (customizeResult ?? (l => l))(_attach(populate(_initialize(Reusable.OmniLog.Log.Empty))));
            if (log.Any() && _canLog(log))
            {
                Log(log);
            }

            return this;
        }

        public ILogger Log(ILog log)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(log);
            }

            return this;
        }

        #endregion

//        #region IEquatable
//
//        public bool Equals(ILogger other) => AutoEquality<ILogger>.Comparer.Equals(this, other);
//
//        public override bool Equals(object obj) => Equals(obj as ILogger);
//
//        public override int GetHashCode() => AutoEquality<ILogger>.Comparer.GetHashCode(this);
//
//        #endregion
//

        #region IObservable<Log>

        public IDisposable Subscribe(IObserver<ILog> observer)
        {
//            return
//                _observers.Add(observer)
//                    ? Disposable.Create(() => _observers.Remove(observer))
//                    : Disposable.Empty;

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

        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(T).ToPrettyString(includeNamespace: false));
        }

        public static ILogger<T> Null => new Noop();

        //public SoftString Name => _logger.Name;

        //public ILogger Log(ILogLevel logLevel, Action<ILog> logAction) => _logger.Log(logLevel, logAction);

        public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default) => _logger.Log(populate);

        public ILogger Log(ILog log) => _logger.Log(log);

        public void Dispose() => _logger.Dispose();

        private class Noop : ILogger<T>
        {
            public Noop() => Name = "Null";

            //public ILogger Log(ILogLevel logLevel, Action<ILog> logAction) => this;
            public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default) => this;

            public ILogger Log(ILog log) => this;

            public SoftString Name { get; }

            public void Dispose() { }
        }
    }

    public class LoggerTransaction : ILogger
    {
        private readonly ILogger _logger;
        private readonly IList<ILog> _logs;

        public LoggerTransaction(ILogger logger)
        {
            _logger = logger;
            _logs = new List<ILog>();
        }

        //public SoftString Name => _logger.Name;

        public ILogger Log(TransformCallback populate, TransformCallback customizeResult = default)
        {
            return _logger.Log(populate, r =>
            {
                if (r.Property<bool>(default, LogProperties.OverrideTransaction.ToString()))
                {
                    return r;
                }
                else
                {
                    _logs.Add(r);
                    return OmniLog.Log.Empty;
                }
            });
        }

        public ILogger Log(ILog log)
        {
            if (log.Property<bool>(LogProperties.OverrideTransaction))
            {
                _logger.Log(log);
            }
            else
            {
                _logs.Add(log);
            }

            return this;
        }

        public void Commit()
        {
            foreach (var log in _logs)
            {
                _logger.Log(log);
            }

            _logs.Clear();
        }

        public void Dispose()
        {
            // You don't want to dispose it here because it'll unwire all listeners.
            // _logger.Dispose();
            _logs.Clear();
        }
    }
}