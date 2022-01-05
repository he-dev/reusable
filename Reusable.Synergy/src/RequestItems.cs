using System;
using System.Diagnostics.CodeAnalysis;
using Reusable.Essentials;

namespace Reusable.Synergy;

public static class RequestItems
{
    public static T GetItem<T>(this IRequest request, string name)
    {
        return
            request.Items.TryGetValue(typeof(T).Name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{name}'.");
    }
    
    public static T GetItemOrDefault<T>(this IRequest request, string name, T fallback)
    {
        return
            request.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : fallback;
    }

    // Sets CacheLifetime.
    public static T CacheLifetime<T>(this T request, TimeSpan lifetime) where T : IRequest
    {
        return request.Also(s => s.Items[nameof(CacheLifetime)] = lifetime);
    }

    // Gets CacheLifetime.
    public static TimeSpan CacheLifetime(this IRequest request)
    {
        return request.GetItemOrDefault(nameof(CacheLifetime), TimeSpan.Zero);
    }

    // Sets Tag.
    public static IRequest<T> Tag<T>(this IRequest<T> request, string value)
    {
        return request.Also(s => s.Items[nameof(Tag)] = value);
    }

    // Gets Tag.
    public static string Tag(this IRequest request)
    {
        // To resolve the pipeline use either a custom tag or the typename.
        return request.GetItemOrDefault(nameof(Tag), request.GetType().Name);
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

