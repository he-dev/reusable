using System;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILoggerScope<out TScope, in TParameter> where TScope : IDisposable
    {
        TScope Push(TParameter parameter);
    }
}