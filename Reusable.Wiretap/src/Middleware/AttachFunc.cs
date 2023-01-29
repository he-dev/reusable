using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachFunc : IMiddleware
{
    public AttachFunc(Func<LogEntry, LogEntry> invoke) => InvokeFunc = invoke;

    private Func<LogEntry, LogEntry> InvokeFunc { get; }

    public void Invoke(LogEntry entry, Action<LogEntry> next) => next(InvokeFunc(entry));
}