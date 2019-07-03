using System;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.OmniLog.Abstractions
{
    public delegate ILog TransformCallback(ILog log);
    
    /// <inheritdoc />
    /// <summary>
    /// The base interface for all loggers. The disposable interface can be used to unsubscribe any observers from the logger. It doesn't use any resources so dispose is optional.
    /// </summary>
    public interface ILogger : IDisposable
    {
        //[AutoEqualityProperty]
        //SoftString Name { get; }

        [NotNull]
        //ILogger Log([NotNull] ILogLevel logLevel, [NotNull] Action<ILog> logAction);
        ILogger Log(TransformCallback populate, TransformCallback customizeResult = default);
        
        ILogger Log([NotNull] ILog log);
    }
    
    public interface ILogger<T> : ILogger { }
}