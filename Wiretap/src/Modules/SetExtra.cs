using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Modules;

public class SetExtra : IModule
{
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

    public void Invoke(TraceContext context, LogAction next)
    {
        foreach (var item in Items)
        {
            if (context.Entry.ContainsKey(item.Key) == false)
            {
                context.Entry.SetItem(item.Key, item.Value);
            }
        }

        next(context);
    }
}