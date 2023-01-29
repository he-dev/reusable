using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Abstractions;

public interface ILogger
{
    void Log(LogEntry entry);
}