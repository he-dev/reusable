using System;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class AttachOwner : IModule
{
    public void Invoke(TraceContext context, LogAction next)
    {
        if (context.Activity.Items.GetItem<Type>(Strings.Items.Owner) is { } owner)
        {
            context.Entry.Details()!.SetItem(Strings.Items.Owner, owner.Name);
        }

        next(context);
    }
}