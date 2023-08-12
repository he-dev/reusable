using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class InvokeFunc : IModule
{
    public InvokeFunc(Func<IActivity, LogEntry, LogEntry> invoke) => Func = invoke;

    private Func<IActivity, LogEntry, LogEntry> Func { get; }

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next) => next(activity, Func(activity, entry));
}