using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(ILogger))]

namespace Reusable.OmniLog
{
    public delegate bool LogPredicate(ILog log);

    public class Logger : ILogger, IObservable<ILog>, IEquatable<ILogger>
    {
        public static readonly IEnumerable<ILogLevel> LogLevels;

        private bool _disposed;
        private readonly SoftString _name;
        private readonly LogPredicate _logPredicate;
        private readonly Action _dispose;
        private readonly ISet<IObserver<ILog>> _observers;

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

        internal Logger([NotNull] SoftString name, [NotNull] LogPredicate logPredicate, Action dispose)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _logPredicate = logPredicate;
            _dispose = dispose;

            _observers = new HashSet<IObserver<ILog>>();
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => _name);
            builder.DisplayMember(x => Attachments.Count);
        });

        internal HashSet<ILogAttachment> Attachments { get; set; } = new HashSet<ILogAttachment>();

        #region ILogger

        public SoftString Name => _name;

        public ILogger Log(ILogLevel logLevel, Action<ILog> logAction)
        {
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            // Not sure whether this exception should be thrown.
            //if (_disposed) throw new InvalidOperationException($"This logger {Name.ToString().EncloseWith("()")} has been unsubscribed and cannot be used anymore.");

            var log = new Log();
            log.Name(_name);
            log.Level(logLevel);
            logAction(log);

            if (_logPredicate(log))
            {
                var rendered = log.Render(Attachments);
                foreach (var observer in _observers)
                {
                    observer.OnNext(rendered);
                }
            }

            return this;
        }

        #endregion

        #region IEquatable

        public bool Equals(ILogger other) => AutoEquality<ILogger>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ILogger);

        public override int GetHashCode() => AutoEquality<ILogger>.Comparer.GetHashCode(this);

        #endregion

        #region IObservable<Log>

        public IDisposable Subscribe(IObserver<ILog> observer)
        {
            return
                _observers.Add(observer)
                    ? Disposable.Create(() => _observers.Remove(observer))
                    : Disposable.Empty;
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
            _logger = loggerFactory.CreateLogger<T>();
        }

        public static ILogger<T> Null => new _Null();

        public SoftString Name => _logger.Name;

        public ILogger Log(ILogLevel logLevel, Action<ILog> logAction) => _logger.Log(logLevel, logAction);

        public void Dispose() => _logger.Dispose();

        private class _Null : ILogger<T>
        {
            public _Null() => Name = "Null";

            public ILogger Log(ILogLevel logLevel, Action<ILog> logAction) => this;

            public SoftString Name { get; }

            public void Dispose() { }
        }
    }
}