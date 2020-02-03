using System;
using System.Collections.Generic;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerFactory : IDisposable, IEnumerable<ILoggerNode>
    {
        ILogger CreateLogger(string name);
    }
}