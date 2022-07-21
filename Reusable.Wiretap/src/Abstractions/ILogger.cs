using System;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger : IEnumerable<ILoggerNode>, IDisposable //: ILoggerNode
{
    void Log(ILogEntry logEntry);
}

// ReSharper disable once UnusedTypeParameter - This is required for dependency-injection.
public interface ILogger<T> : ILogger { }