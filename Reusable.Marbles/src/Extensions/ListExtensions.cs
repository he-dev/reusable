using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Marbles.Extensions;

public static class ListExtensions
{
    /// <summary>
    /// Adds the specified item or replaces it if it matches the key.
    /// </summary>
    public static void Add<TKey, TValue>(this IList<TValue> source, TValue item, Func<TValue, TKey> selectKey)
    {
        foreach (var (x, i) in source.Select((x, i) => (x, i)))
        {
            if (Equals(selectKey(x), selectKey(item)))
            {
                source[i] = item;
                return;
            }
        }

        source.Add(item);
    }
}