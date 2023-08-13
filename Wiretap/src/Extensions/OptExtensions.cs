using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Extensions;

public abstract record Opt(Type ModuleType)
{
    public record In(Type ModuleType) : Opt(ModuleType);

    public record Out(Type ModuleType) : Opt(ModuleType);
}

public static class OptExtensions
{
    public static IDisposable PushOptIn<T>(this ActivityContext activity) where T : ILog => activity.Items.PushItem(nameof(Opt), new Opt.In(typeof(T)));
    public static IDisposable PushOptOut<T>(this ActivityContext activity) where T : ILog => activity.Items.PushItem(nameof(Opt), new Opt.Out(typeof(T)));
    public static Opt? Opt(this IDictionary<string, object?> source) => source.GetItemByCaller<Opt>();
}