using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Container = System.Collections.Generic.IDictionary<string, object?>;

namespace Reusable.Wiretap.Extensions;

public static class LogEntryItems
{
    public static Container Module(this Container source, IModule module) => source.SetItemByCaller(module);
    public static IModule? Module(this Container source) => source.GetItemByCaller<IModule>();

    public static Container Elapsed(this Container source, double value) => source.SetItemByCaller(value);

    public static object UniqueId(this Container source) => source.GetItemByCaller<object>() ?? new InvalidOperationException("The unique-id doesn't seem to be initialized yet.");
    public static object UniqueId(this Container source, Func<object> factory) => source.GetItemOrCreate<string, object?>(nameof(UniqueId), factory)!;
    public static Container UniqueId(this Container source, object? value) => source.SetItemByCaller(value);
    public static Container ParentId(this Container source, object? value) => source.SetItemByCaller(value);

    public static DateTime? Timestamp(this Container source) => source.GetItemByCaller<DateTime>();
    public static Container Timestamp(this Container source, DateTime value) => source.SetItemByCaller(value);

    public static IDictionary<string, object?>? Details(this Container source) => source.GetItemByCaller<IDictionary<string, object?>>();

    public static Stopwatch Stopwatch(this Container source, Func<Stopwatch> factory) => source.GetItemOrCreate(nameof(Stopwatch), factory)!;


    public static Exception? Exception(this Container source) => source.GetItemByCaller<Exception>();
    public static Container Exception(this Container source, Exception value) => source.SetItemByCaller(value);


}