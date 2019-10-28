using System;
using JetBrains.Annotations;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerNodeScope<out TScope, in TParameter> where TScope : IDisposable
    {
        [CanBeNull]
        TScope Current { get; }
        
        TScope Push(TParameter parameter);
    }
}