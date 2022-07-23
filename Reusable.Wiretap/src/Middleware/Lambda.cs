using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Middleware;

public class Lambda : LoggerMiddleware
{
    public Lambda(Action<ILogEntry> action) => Action = action;

    private Action<ILogEntry> Action { get; }

    public override void Invoke(ILogEntry entry)
    {
        Action(entry);
        Next?.Invoke(entry);
    }
}