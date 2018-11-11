using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.OmniLog;

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

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x._cache.Count);
            builder.Property(x => x.Configuration.Attachements.Count);
        });

        [NotNull]
        public IList<ILogRx> Observers { get; set; } = new List<ILogRx>();

        [NotNull]
        public LoggerFactoryConfiguration Configuration { get; set; } = new LoggerFactoryConfiguration();

        #region ILoggerFactory

        public ILogger CreateLogger([NotNull] SoftString name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _cache.GetOrAdd(name, CreateLoggerInternal).Logger;
        }

        private (ILogger Logger, IDisposable Unsubscriber) CreateLoggerInternal(SoftString name)
        {
            void UnsubscribeLogger()
            {
                if (_cache.TryRemove(name, out var item))
                {
                    item.Subscriptions.Dispose();
                }
            }

            var logger = new Logger(name, Configuration.LogPredicate, UnsubscribeLogger)
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
        }

        public void Dispose()
        {
            foreach (var item in _cache)
            {
                item.Value.Logger.Dispose();
            }
            _cache.Clear();

            foreach (var attachement in Configuration.Attachements)
            {
                if (attachement is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        #endregion
    }
}