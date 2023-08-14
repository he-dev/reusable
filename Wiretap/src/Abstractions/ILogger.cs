using System;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger
{
    Type Owner { get; }

    LogAction Log { get; }
}

public interface ILogger<T> : ILogger { }

public delegate void LogAction(TraceContext context);

/// <summary>
/// This is a marker interface for log modules.
/// </summary>
public interface ILog : IModule { }