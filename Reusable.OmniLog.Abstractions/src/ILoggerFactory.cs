using System;
using System.Collections.Generic;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(string name);
    }
}