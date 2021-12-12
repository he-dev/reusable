using System.Collections;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger : ILoggerNode
{
    string Name { get; init; }
    
    void Log(ILogEntry logEntry);
}

// ReSharper disable once UnusedTypeParameter - This is required for dependency-injection.
public interface ILogger<T> : ILogger { }