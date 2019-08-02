using System;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerFactory : IObservable<ILog>, IDisposable
    {
        ILogger CreateLogger(SoftString name);
    }
}