using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(LoggerFactory))]

namespace Reusable.OmniLog
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(SoftString name);
    }

    [PublicAPI]
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<SoftString, (ILogger Logger, IDisposable Subscriptions)> _cache;

        //private readonly IEnumerable<IObserver<Log>> _observers;

        public LoggerFactory()
        {
            _cache = new ConcurrentDictionary<SoftString, (ILogger Logger, IDisposable Subscriptions)>();
        }

        private string DebuggerDisplay() => DebuggerDisplayHelper<LoggerFactory>.ToString(this, builder =>
        {
            builder.Property(x => x._cache.Count);
            builder.Property(x => x.Configuration.Attachements.Count);
        });

        [NotNull]
        public IList<ILogRx> Observers { get; set; } = new List<ILogRx>();

        [NotNull]
        public LoggerConfiguration Configuration { get; set; } = new LoggerConfiguration();

        private bool LoggerMatches(LogLevel logLevel, SoftString category) => Configuration.LoggerPredicate(logLevel, category);

        private bool LogMatches(Log log) => Configuration.LogPredicate(log);

        #region ILoggerFactory

        public ILogger CreateLogger([NotNull] SoftString name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _cache.GetOrAdd(name, n =>
            {
                void UnsubscribeLogger()
                {
                    if (_cache.TryRemove(name, out var item))
                    {
                        item.Subscriptions.Dispose();
                    }
                }

                var logger = new Logger(name, LoggerMatches, LogMatches, UnsubscribeLogger)
                {
                    Attachements = Configuration.Attachements
                };

                var subscriptions = Observers.Select(logger.Subscribe).ToList();

                void Unsubscribe()
                {
                    foreach (var subscription in subscriptions)
                    {
                        subscription.Dispose();
                    }
                }

                return (logger, Disposable.Create(Unsubscribe));
            }).Logger;
        }

        public void Dispose()
        {
            foreach (var item in _cache)
            {
                item.Value.Logger.Dispose();
            }
            _cache.Clear();
        }

        #endregion
    }
}