using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(LoggerFactory))]

namespace Reusable.OmniLog
{
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

        public static LoggerFactory Empty => new LoggerFactory();

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x._cache.Count);
            //builder.DisplayValue(x => x.Configuration.Attachments.Count);
        });

        [NotNull]
        public IList<ILogRx> Observers { get; set; } = new List<ILogRx>();

        //[NotNull]
        //public LoggerFactoryConfiguration Configuration { get; set; } = new LoggerFactoryConfiguration();
        
        [NotNull]
        public HashSet<ILogAttachment> Attachments { get; set; } = new HashSet<ILogAttachment>();

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

            var logger = new Logger
            (
                initialize: log => log.SetItem(LogPropertyNames.Name, name.ToString()),
                attach: log => log.Render(Attachments),
                canLog: _ => true,
                dispose: UnsubscribeLogger
            );

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

            foreach (var attachment in Attachments)
            {
                if (attachment is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        #endregion
    }
}