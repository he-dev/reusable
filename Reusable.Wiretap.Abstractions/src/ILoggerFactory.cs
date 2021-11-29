using System;

namespace Reusable.Wiretap.Abstractions
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(string name);
    }
}