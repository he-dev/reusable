using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Extensions;

public static class DictionaryExtensions
{
    public static TItem? GetItemOrCreate<TKey, TItem>(this IDictionary<TKey, TItem?> source, TKey key, Func<TItem?> factory)
    {
        if (source.TryGetValue(key, out var result))
        {
            return result;
        }

        return source[key] = factory();
    }

    public static TItem? GetItemOrCreate<TSource, TItem>(this TSource source, string key, Func<TItem?> factory) where TSource : IDictionary<string, object?>
    {
        if (source.TryGetValue(key, out var value))
        {
            if (value is TItem result)
            {
                return result;
            }

            if (value is not null and not TItem)
            {
                throw new ArgumentException(message: $"Item '{key}' is of type '{value.GetType()}', but '{typeof(TItem)}' was requested.");
            }
        }

        return (TItem?)(source[key] = factory());
    }

    public static async Task<TItem?> GetItemOrCreate<TSource, TItem>(this TSource source, string key, Func<Task<TItem?>> factory) where TSource : IDictionary<string, object?>
    {
        if (source.TryGetValue(key, out var value))
        {
            if (value is TItem result)
            {
                return result;
            }

            if (value is not null and not TItem)
            {
                throw new ArgumentException(message: $"Item '{key}' is of type '{value.GetType()}', but '{typeof(TItem)}' was requested.");
            }
        }

        return (TItem?)(source[key] = await factory());
    }

    public static TItem? GetItem<TItem>(this IDictionary<string, object?> source, string key)
    {
        return
            source.TryGetValue(key, out var value) && value is TItem result
                ? result
                : default;
    }

    public static T? GetItemByCaller<T>(this IDictionary<string, object?> source, [CallerMemberName] string name = "") => source.GetItem<T>(name);

    public static TSource SetItem<TSource>(this TSource source, string key, object? value) where TSource : IDictionary<string, object?>
    {
        return source.Also(dict => dict[key] = value);
    }

    public static TSource SetItemByCaller<TSource, T>(this TSource source, T value, [CallerMemberName] string name = "") where TSource : IDictionary<string, object?>
    {
        return source.Also(c => c.SetItem(name, value));
    }
    
    public static TSource RemoveItem<TSource>(this TSource source, string key) where TSource : IDictionary<string, object?>
    {
        return source.Also(dict => dict.Remove(key));
    }
}