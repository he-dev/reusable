using System;
using Reusable.Extensions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Services;

public interface IFilter
{
    bool Matches(TraceContext context);
}

public class FuncFilter : IFilter
{
    public FuncFilter(Func<TraceContext, bool> matchesFunc) => MatchesFunc = matchesFunc;

    private Func<TraceContext, bool> MatchesFunc { get; }

    public bool Matches(TraceContext context) => MatchesFunc(context);
}

public class TraceFilter : IFilter
{
    public required string Trace { get; init; }

    public bool Matches(TraceContext context)
    {
        return context.Entry.GetItem<string>(Strings.Items.Trace)?.Equals(Trace, StringComparison.OrdinalIgnoreCase) == true;
    }
}

public class OptFilter : IFilter
{
    public bool Matches(TraceContext context)
    {
        if (context.Items.Opt() is { } opt && context.Items.Module() is { } module)
        {
            var moduleType = module.GetType();
            return
                (opt is Opt.In && moduleType == opt.ModuleType) ||
                (opt is Opt.Out && moduleType != opt.ModuleType);
        }

        return true;
    }
}