using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class AttachDetails : IModule
{
    public void Invoke(TraceContext context, LogFunc next)
    {
        foreach (var detail in context.Items.Details())
        {
            context.Entry.Details().SetItem(detail.Key, detail.Value);
        }

        next(context);
    }
}