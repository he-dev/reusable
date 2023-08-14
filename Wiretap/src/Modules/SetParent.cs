using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class SetParent : IModule
{
    public void Invoke(TraceContext context, LogAction next)
    {
        if (context.Activity.Skip(1).FirstOrDefault() is { } parent)
        {
            context.Entry.ParentId(parent.Items.UniqueId());
        }

        next(context);
    }
}