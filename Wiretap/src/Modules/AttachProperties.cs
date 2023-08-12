using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class AttachProperties : IModule
{
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        next(activity, Items.Aggregate(entry, (current, property) => current.SetItem(property.Key, property.Value)));
    }
}