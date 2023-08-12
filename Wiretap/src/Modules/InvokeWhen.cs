using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class InvokeWhen : IModule
{
    public required IModule Module { get; init; }

    public required IModuleFilter Filter { get; init; }

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (Filter.Matches(activity, entry.Module(Module)))
        {
            Module.Invoke(activity, entry, next);
        }
        else
        {
            next(activity, entry);
        }
    }
}

public interface IModuleFilter
{
    bool Matches(IActivity activity, LogEntry entry);
}

public class OptFilter : IModuleFilter
{
    public bool Matches(IActivity activity, LogEntry entry)
    {
        if (activity.Items.Opt() is { } opt && entry.Module() is { } module)
        {
            var moduleType = module.GetType();
            return
                (opt is Opt.In && moduleType == opt.ModuleType) ||
                (opt is Opt.Out && moduleType != opt.ModuleType);
        }

        return true;
    }
}