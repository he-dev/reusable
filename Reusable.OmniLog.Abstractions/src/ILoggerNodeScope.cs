using System;
using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerNodeScope<out TScope, in TParameter> where TScope : class, IDisposable
    {
        TScope? Current { get; }

        TScope Push(TParameter parameter);
    }
}