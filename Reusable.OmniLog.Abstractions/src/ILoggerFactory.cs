using System;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(SoftString name);
    }
}