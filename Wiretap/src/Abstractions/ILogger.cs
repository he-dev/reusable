using System.Runtime.CompilerServices;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger
{
    LoggerContext Start
    (
        string name,
        object? details = default,
        object? attachment = default
    );
}

public delegate void LogDelegate(LogEntry entry);