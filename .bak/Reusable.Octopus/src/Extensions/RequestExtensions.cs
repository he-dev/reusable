using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus.Extensions;

public static class RequestExtensions
{
    public static T GetItemOrDefault<T>(this Request request, string name, T defaultValue)
    {
        return
            request.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : defaultValue;
    }

    public static T GetItem<T>(this Request request, string name)
    {
        return
            request.Items.TryGetValue(name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{name}'.");
    }
    
    public static T GetItem<T>(this Request request) where T : Enum
    {
        return 
            request.Items.TryGetValue(typeof(T).Name, out var value) && value is T result
                ? result
                : throw DynamicException.Create("ItemNotFound", $"Could not find item '{typeof(T).Name}'.");
    }

    public static bool ItemEquals<T>(this Request request, string name, T other, IEqualityComparer<T>? comparer = default)
    {
        comparer ??= EqualityComparer<T>.Default;
        return request.Items.TryGetValue(name, out var item) && item is T value && comparer.Equals(value, other);
    }
    
    public static Request SetItem<T>(this Request request, T value) where T : Enum
    {
        return request.Also(r => r.Items[typeof(T).Name] = value);
    }
    
    public static bool TryGetItem<T>(this Request request, [MaybeNullWhen(false)] out T result) where T : Enum
    {
        if (request.Items.TryGetValue(typeof(T).Name, out var value) && value is T actual)
        {
            result = actual;
            return true;
        }

        result = default;
        return false;
    }

    public static void Required(this Request request, bool required)
    {
        request.Items[nameof(Required)] = required;
    }

    public static bool Required(this Request request)
    {
        return request.Items.TryGetValue(nameof(Required), out var value) && value is true;
    }

    public static void CacheLifetime(this Request request, TimeSpan lifetime)
    {
        request.Items[nameof(CacheLifetime)] = lifetime;
    }

    public static TimeSpan CacheLifetime(this Request request)
    {
        return request.Items.TryGetValue(nameof(CacheLifetime), out var value) && value is TimeSpan lifetime ? lifetime : TimeSpan.Zero;
    }

    public static T Log<T>(this T source, string message) where T : IItems
    {
        return source.Also(r =>
        {
            if (r.Items.TryGetValue(nameof(Log), out var item) && item is IList<string> logs)
            {
                logs.Add(CreateLogEntry<T>(message));
            }
            else
            {
                r.Items[nameof(Log)] = new List<string> { CreateLogEntry<T>(message) };
            }
        });
    }

    private static string CreateLogEntry<T>(string message) => $"{DateTime.UtcNow} | {typeof(T).ToPrettyString()} | {message}";
}