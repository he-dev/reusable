using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Flexo;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;

[assembly: DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes, Target = typeof(LoggerFactory))]

namespace Reusable.OmniLog
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class LoggerFactory : ILoggerFactory
    {
        private readonly ISubject<ILog> _subject;
        private readonly ConcurrentDictionary<SoftString, ILogger> _loggers;
        private readonly IList<IDisposable> _subscriptions;

        public LoggerFactory()
        {
            _subject = new Subject<ILog>();
            _loggers = new ConcurrentDictionary<SoftString, ILogger>();
            _subscriptions = new List<IDisposable>();
        }

        public static LoggerFactory Empty => new LoggerFactory();

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x._loggers.Count);
            //builder.DisplayValue(x => x.Configuration.Attachments.Count);
        });

        [NotNull]
        public HashSet<ILogAttachment> Attachments { get; set; } = new HashSet<ILogAttachment>();

        #region ILoggerFactory

        public IDisposable Subscribe(IObserver<ILog> observer) => _subject.Where(Any).Subscribe(observer);

        /// <summary>
        /// Allows to subscribe to this factory so that it maintains the subscription.
        /// </summary>
        public LoggerFactory Subscribe(IObserver<ILog> observer, Func<IObservable<ILog>, IObservable<ILog>> configure)
        {
            _subscriptions.Add(configure(_subject).Where(Any).Subscribe(observer));
            return this;
        }

        public ILogger CreateLogger([NotNull] SoftString name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return _loggers.GetOrAdd(name, n => new Logger
            (
                initialize: log => log.SetItem(LogPropertyNames.Name, n.ToString()),
                attach: log => log.Render(Attachments),
                observer: _subject
            ));
        }

        public void Dispose()
        {
            foreach (var attachment in Attachments)
            {
                if (attachment is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        #endregion

        private static Func<ILog, bool> Any => l => l.Any();
    }
}