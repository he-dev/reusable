using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class AttachCallerInfo : IModule
{
    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (activity.Items.GetItem("CallerInfo") is { } callerInfo)
        {
            var details = entry.Details().SetItem("Caller", callerInfo);
            entry = entry.SetItem(LogEntry.PropertyNames.Details, details);
        }

        next(activity, entry);
    }
}