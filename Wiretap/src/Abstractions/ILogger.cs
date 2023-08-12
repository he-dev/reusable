using System.Runtime.CompilerServices;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger
{
    ActivityContext Begin
    (
        string name,
        string? message = default,
        object? details = default,
        object? attachment = default
    );
}


public delegate void LogFunc(IActivity activity, LogEntry entry);

/// <summary>
/// This is a marker interface for log modules.
/// </summary>
public interface ILog : IModule {}