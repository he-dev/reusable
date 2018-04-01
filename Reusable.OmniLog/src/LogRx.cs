using System;
using System.Text.RegularExpressions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public interface ILogRx : IObserver<Log> { }

    public abstract class LogRx : ILogRx
    {
        private readonly Lazy<IObserver<Log>> _logRx;

        protected LogRx()
        {
            _logRx = new Lazy<IObserver<Log>>(Initialize);
            Name = Regex.Replace(GetType().Name, "Rx$", string.Empty);
        }

        public SoftString Name { get; }

        protected abstract IObserver<Log> Initialize();

        public virtual void OnNext(Log value) => _logRx.Value.OnNext(value);

        public virtual void OnError(Exception error) => _logRx.Value.OnError(error);

        public virtual void OnCompleted() => _logRx.Value.OnCompleted();
    }
}