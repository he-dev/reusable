using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Extensions;


namespace Reusable.Wiretap.Data;

public static class ActivityContextItems
{
    public static T? GetItem<T>(this IMemoryCache cache, [CallerMemberName] string name = "") => cache.TryGetValue<T>(name, out var value) ? value : default;
    public static object? GetItem(this IMemoryCache cache, string name) => cache.TryGetValue<object>(name, out var value) ? value : default;
    private static IMemoryCache SetItem<T>(this IMemoryCache cache, T value, [CallerMemberName] string name = "") => cache.Also(c => c.Set(name, value));

    public static Stopwatch Stopwatch(this IMemoryCache cache, Func<ICacheEntry, Stopwatch> factory) => cache.GetOrCreate(nameof(Stopwatch), factory)!;

    public static object UniqueId(this IMemoryCache cache, Func<ICacheEntry, object> factory) => cache.GetOrCreate(nameof(UniqueId), factory)!;
    public static object? UniqueId(this IMemoryCache cache) => cache.GetItem<object>();

    public static Exception? Exception(this IMemoryCache cache) => cache.GetItem<Exception>();
    public static IMemoryCache Exception(this IMemoryCache cache, Exception value) => cache.SetItem(value);
}