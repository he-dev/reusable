using System;
using Reusable.Essentials;

namespace Reusable.Synergy;

public static class RequestItems
{
    public static T GetItem<T>(this IItems service, string name)
    {
        return
            service.Items.TryGetValue(typeof(T).Name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{name}'.");
    }

    public static T GetItemOrDefault<T>(this IItems service, string name, T fallback)
    {
        return
            service.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : fallback;
    }

    // Sets CacheLifetime.
    public static IService<T> CacheLifetime<T>(this IService<T> service, TimeSpan lifetime)
    {
        return service.Also(s => s.Items[nameof(CacheLifetime)] = lifetime);
    }

    // Gets CacheLifetime.
    public static TimeSpan CacheLifetime<T>(this T service) where T : IItems
    {
        return service.GetItemOrDefault(nameof(CacheLifetime), TimeSpan.Zero);
    }

    // Sets Tag.
    public static IService<T> Tag<T>(this IService<T> service, string value)
    {
        return service.Also(s => s.Items[nameof(Tag)] = value);
    }

    // Gets Tag.
    public static string Tag<T>(this T service) where T : IItems
    {
        // To resolve the pipeline use either a custom tag or the typename.
        return service.GetItemOrDefault(nameof(Tag), service.GetType().Name);
    }
    
    public static T GetItem<T>(this IRequest request) where T : Enum
    {
        return 
            request.Items.TryGetValue(typeof(T).Name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{typeof(T).Name}'.");
    }

    public static IRequest SetItem<T>(this IRequest request, T value) where T : Enum
    {
        return request.Also(r  => r.Items[typeof(T).Name] = value);
    }
}

public enum OnError
{
    Halt,
    Next,
}