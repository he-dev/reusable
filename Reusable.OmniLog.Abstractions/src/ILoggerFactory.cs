using System;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(string name);
    }
}