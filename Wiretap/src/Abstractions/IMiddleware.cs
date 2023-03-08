using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Abstractions;

public interface IMiddleware
{
    void Invoke(LogEntry entry, LogDelegate next);
}

