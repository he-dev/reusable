using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Middleware;

public class Lambda : LoggerMiddleware
{
    private readonly Action<ILogEntry, ILoggerMiddleware?> invoke;
    
    public Lambda(Action<ILogEntry, ILoggerMiddleware?> invoke) => this.invoke = invoke;
    
    public override void Invoke(ILogEntry entry) => invoke(entry, Next);
}