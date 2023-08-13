using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Modules;

public class InvokeAction : IModule
{
    public required Action<TraceContext> Body { get; init; }

    public void Invoke(TraceContext context, LogFunc next)
    {
        Body(context);
        next(context);
    }
}