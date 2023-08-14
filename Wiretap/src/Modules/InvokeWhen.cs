using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class InvokeWhen : IModule
{
    public required IModule Module { get; init; }

    public required IFilter Filter { get; init; }

    public void Invoke(TraceContext context, LogAction next)
    {
        using var moduleToken = context.Items.PushItem(Strings.Items.Module, Module);
        if (Filter.Matches(context))
        {
            moduleToken.Dispose();
            Module.Invoke(context, next);
        }
        else
        {
            moduleToken.Dispose();
            next(context);
        }
    }
}