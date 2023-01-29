using System;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Abstractions;

public interface IMiddleware
{
    void Invoke(LogEntry entry, Action<LogEntry> next);
}

