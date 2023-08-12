using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class SetParent : IModule
{
    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (activity.Skip(1).FirstOrDefault() is { } context)
        {
            entry = entry.UniqueId(context.Items.UniqueId());
        }

        next(activity, entry);
    }
}