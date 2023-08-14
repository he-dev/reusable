using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class SetTimestamp : IModule
{
    public Func<DateTime> Now { get; set; } = () => DateTime.UtcNow;

    public void Invoke(TraceContext context, LogAction next)
    {
        context.Entry.Timestamp(Now());
        next(context);
    }
}