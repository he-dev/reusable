using System;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class SetUniqueId : IModule
{
    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewId { get; set; } = () => Guid.NewGuid();

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        var id = activity.Items.UniqueId(_ => NewId());
        next(activity, entry.UniqueId(id));
    }
}