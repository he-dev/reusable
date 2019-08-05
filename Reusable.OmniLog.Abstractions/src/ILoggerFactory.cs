using System;
using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerFactory : IDisposable
    {
        [NotNull]
        ILogger CreateLogger([NotNull] string name);
    }
}