using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class InvokeWhen : IModule
{
    public required IModule Module { get; init; }

    public required IFilter Filter { get; init; }

    public void Invoke(TraceContext context, LogFunc next)
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

public interface IFilter
{
    bool Matches(TraceContext context);
}

public class OptFilter : IFilter
{
    public bool Matches(TraceContext context)
    {
        if (context.Items.Opt() is { } opt && context.Items.Module() is {} module)
        {
            var moduleType = module.GetType();
            return
                (opt is Opt.In && moduleType == opt.ModuleType) ||
                (opt is Opt.Out && moduleType != opt.ModuleType);
        }

        return true;
    }
}