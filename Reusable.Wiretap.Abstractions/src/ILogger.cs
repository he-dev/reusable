using System;
using System.Collections.Generic;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogger : ILoggerNode
    {
        void Log(ILogEntry logEntry);
    }

    // ReSharper disable once UnusedTypeParameter - This is required for dependency-injection.
    public interface ILogger<T> : ILogger { }

    //public interface ILoggerScope : ILogger { }
    public interface ILoggerScope : IDisposable, IEnumerable<ILoggerScope>
    {
        ILoggerNode First { get; }
        
        Stack<(Exception Exception, Data.CallSite CallSite)> Exceptions { get; }
    }
}