using System;
using System.Reactive;
using System.Text.RegularExpressions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public interface ILogRx : IObserver<ILog>
    {
        void Log(ILog log);
    }

    public abstract class LogRx : ILogRx
    {
        private readonly IObserver<ILog> _log;

        protected LogRx()
        {
            _log = Observer.Create<ILog>(Log);
            Name = Regex.Replace(GetType().Name, "Rx$", string.Empty);
        }

        public SoftString Name { get; }

        public abstract void Log(ILog log);

        public virtual void OnNext(ILog value) => _log.OnNext(value);

        public virtual void OnError(Exception error) => _log.OnError(error);

        public virtual void OnCompleted() => _log.OnCompleted();
    }
}