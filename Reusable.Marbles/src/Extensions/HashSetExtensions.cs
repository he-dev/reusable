using System.Collections.Generic;

namespace Reusable.Marbles.Extensions;

public static class HashSetExtensions
{
    public static bool AddRange<T>(this ISet<T> set, IEnumerable<T> source)
    {
        var addedCount = 0;
        var itemCount = 0;
        foreach (var item in source)
        {
            addedCount += set.Add(item) ? 1 : 0;
            itemCount++;
        }

        return addedCount == itemCount;
    }
}