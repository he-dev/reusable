using System;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.OmniLog.Abstractions
{
    //public delegate ILog TransformCallback(ILog log);

    /// <summary>
    /// The base interface for all loggers. The disposable interface can be used to unsubscribe any observers from the logger. It doesn't use any resources so dispose is optional.
    /// </summary>
    public interface ILogger //: IDisposable
    {
        // You need customizeResult so that a decorator can intercept the result.
        [NotNull]
        ILogger Log(Func<ILog, ILog> request, Func<ILog, ILog> response = default);

        [NotNull]
        ILogger Log([NotNull] ILog log);
    }

    // You need T here because you use it for dependency injection
    // to automatically create a name for the logger from T. 
    public interface ILogger<T> : ILogger { }
}