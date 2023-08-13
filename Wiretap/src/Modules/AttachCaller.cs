using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Extensions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class AttachCaller : IModule
{
    public void Invoke(TraceContext context, LogFunc next)
    {
        if (context.Activity.Items.GetItem<object>(Strings.Items.Caller) is { } caller)
        {
            context.Entry.Details().SetItem(Strings.Items.Caller, caller);
        }

        next(context);
    }
}