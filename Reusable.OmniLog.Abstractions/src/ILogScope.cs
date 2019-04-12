using System;

namespace Reusable.OmniLog.Abstractions {
    public interface ILogScope : ILog, IDisposable
    {
        int Depth { get; }
    }
}