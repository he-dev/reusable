using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Text.RegularExpressions;

namespace Reusable.OmniLog
{
    public interface ILogRx : IObserver<Log> { }

    public abstract class LogRx : ILogRx
    {
        private readonly IObserver<Log> _log;

        protected LogRx()
        {
            _log = Observer.Create<Log>(Log);
            Name = Regex.Replace(GetType().Name, "Rx$", string.Empty);
        }

        public SoftString Name { get; }

        protected abstract void Log(Log log);

        public virtual void OnNext(Log value) => _log.OnNext(value);

        public virtual void OnError(Exception error) => _log.OnError(error);

        public virtual void OnCompleted() => _log.OnCompleted();
    }
}