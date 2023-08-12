using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class SetTimestamp : IModule
{
    public Func<DateTime> Now { get; set; } = () => DateTime.UtcNow;

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        next(activity, entry.Timestamp(Now()));
    }
}