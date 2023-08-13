using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class SetUniqueId : IModule
{
    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewId { get; set; } = () => Guid.NewGuid();

    public void Invoke(TraceContext context, LogFunc next)
    {
        var id = context.Activity.Items.UniqueId(NewId);
        context.Entry.UniqueId(id);
        next(context);
    }
}