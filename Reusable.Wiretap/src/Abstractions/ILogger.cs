using System;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger : ILoggerMiddleware
{
    string Name { get; }

    void Log(ILogEntry entry);
}

// ReSharper disable once UnusedTypeParameter - This is required for dependency-injection.
public interface ILogger<T> : ILogger { }