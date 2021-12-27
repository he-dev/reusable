using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Collections;
using Reusable.Essentials.Extensions;

// ReSharper disable once CheckNamespace
namespace System.Linq.Custom;

[PublicAPI]
// ReSharper disable once InconsistentNaming - I want this lower case because otherwise it conflicts with the .net class.
public static class enumerable
{
    /// <summary>
    /// Applies a specified function to the corresponding elements of two sequences, producing a sequence of the results.
    /// Unlike the default Zip method this one doesn't stop if enumerating one of the collections is complete.
    /// It continues enumerating the longer collection and return default(T) for the other one.
    /// </summary>
    /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
    /// <param name="first">The first input sequence.</param>
    /// <param name="second">The second input sequence.</param>
    /// <param name="resultSelector">A function that specifies how to combine the corresponding elements of the two sequences.</param>
    /// <returns>An IEnumerableOfT that contains elements of the two input sequences, combined by resultSelector.</returns>
    public static IEnumerable<TResult> ZipOrDefault<TFirst, TSecond, TResult>
    (
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TFirst, TSecond?, TResult?> resultSelector
    )
    {
        using var x = first.GetEnumerator();
        using var y = second.GetEnumerator();

        var xm = false;
        var ym = false;
        while ((xm = x.MoveNext()) | (ym = y.MoveNext()))
        {
            yield return resultSelector
            (
                xm ? x.Current : default,
                ym ? y.Current : default
            );
        }
    }

    public static IEnumerable<(TFirst, TSecond)> ZipOrDefault<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
    {
        return first.ZipOrDefault(second, (f, s) => (f, s));
    }

#if NET47
        [NotNull]
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            return enumerable.Concat(new[] { item });
        }
#endif

#if NET47
        [NotNull]
        public static IEnumerable<T> Prepend<T>([NotNull] this IEnumerable<T> source, T item)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new[] { item }.Concat(source);
        }
#endif

    public static string Join<T>(this IEnumerable<T> values, string separator = "")
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        return string.Join(separator, values);
    }

    public static string Join<T>(this IEnumerable<T> values, Func<T, string> selector, string separator)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        return string.Join(separator, values.Select(selector));
    }

    public static IEnumerable<TSource> Except<TSource, T>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, T> keySelector)
    {
        return first.Except(second, EqualityComparer.Create(keySelector));
    }

    public static IEnumerable<TSource> Except<TSource, T>
    (
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second,
        Func<TSource, T> keySelector,
        IEqualityComparer<T> keyComparer
    )
    {
        return first.Except(second, EqualityComparer.Create(keySelector, keyComparer));
    }

    public static IEnumerable<string> QuoteAllWith<T>(this IEnumerable<T> values, string quotationMark)
    {
        return values.Select(x => (x?.ToString() ?? string.Empty).QuoteWith(quotationMark));
    }

    public static IEnumerable<string> QuoteAllWith<T>(this IEnumerable<T> values, char quotationMark)
    {
        return values.Select(x => (x?.ToString() ?? string.Empty).QuoteWith(quotationMark));
    }

    [ContractAnnotation("source: null => halt")]
    public static IEnumerable<T> Loop<T>(this IEnumerable<T> source)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        var enumerator = source.GetEnumerator();

        try
        {
            // ReSharper disable once PossibleMultipleEnumeration
            while (enumerator.TryMoveNext(source, out enumerator))
            {
                yield return enumerator.Current;
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    private static bool TryMoveNext<T>(this IEnumerator<T> currentEnumerator, IEnumerable<T> source, out IEnumerator<T> nextEnumerator)
    {
        if (currentEnumerator.MoveNext())
        {
            nextEnumerator = currentEnumerator;
            return true;
        }
        else
        {
            // Get the enumerator because we took all elements and try again.
            currentEnumerator.Dispose();
            
            nextEnumerator = source.GetEnumerator();

            // If we couldn't move after reset then we're done trying because the collection is empty.
            return nextEnumerator.MoveNext();
        }
    }

    /// <summary>
    /// Determines if the first collection starts with the second collection.
    /// </summary>
    public static bool StartsWith<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
        using var f = first.GetEnumerator();
        using var s = second.GetEnumerator();

        while (s.MoveNext())
        {
            if (!f.MoveNext()) return false; // First collection is shorter.
            if (!comparer.Equals(f.Current, s.Current)) return false; // Elements don't match.
        }

        // There's nothing more to check.
        return !s.MoveNext();
    }

    public static bool StartsWith<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
        return first.StartsWith(second, EqualityComparer<TSource>.Default);
    }

    public static bool Empty<TSource>(this IEnumerable<TSource> source)
    {
        using var e = source.GetEnumerator();
        return !e.MoveNext();
    }

    public static bool EmptyOr<TSource>(this IEnumerable<TSource> source, Predicate<IEnumerable<TSource>> predicate)
    {
        using var e = source.GetEnumerator();
        return !e.MoveNext() || predicate(source);
    }

    public static IEnumerable<T> Skip<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return source.Where(x => !predicate(x));
    }

    public static Func<TElement, bool> ToAny<TElement>(this IEnumerable<Func<TElement, bool>> filters)
    {
        return x => filters.Any(f => f(x));
    }

    public static T SingleOrThrow<T>(this IEnumerable<T> source, string message)
    {
        using var e = source.GetEnumerator();
        try
        {
            if (e.MoveNext())
            {
                return e.Current;
            }

            throw DynamicException.Create("Empty", message);
        }
        finally
        {
            if (e.MoveNext())
            {
                throw DynamicException.Create("Many", message);
            }
        }
    }

    public static T FirstOrThrow<T>(this IEnumerable<T> source, string message)
    {
        using var e = source.GetEnumerator();
        if (e.MoveNext())
        {
            return e.Current;
        }

        throw DynamicException.Create("Empty", message);
    }

    public static int CalcHashCode<T>(this IEnumerable<T?> values)
    {
        unchecked
        {
            return values.Aggregate(0, (current, next) => (current * 397) ^ next?.GetHashCode() ?? 0);
        }
    }

    public static IEnumerable<T?> Repeat<T>(T item)
    {
        while (true)
        {
            yield return item;
        }

        // ReSharper disable once IteratorNeverReturns - Since it's 'Always' this is by design.
    }

    public static IEnumerable<T> Repeat<T>(Func<T> get)
    {
        while (true)
        {
            yield return get();
        }

        // ReSharper disable once IteratorNeverReturns - Since it's 'Always' this is by design.
    }

    public static bool In<T>(this T value, IEnumerable<T> others, IEqualityComparer<T> comparer)
    {
        return others.Contains(value, comparer);
    }

    public static bool In<T>(this T value, IEnumerable<T> others)
    {
        return value.In(others, EqualityComparer<T>.Default);
    }

    public static bool In<T>(this T value, params T[] others)
    {
        return value.In(others.AsEnumerable());
    }

    public static bool SoftIn(this string? value, IEnumerable<string> others)
    {
        return value.In(others, SoftString.Comparer);
    }

    public static bool SoftIn(this string? value, params string[] others) => value.SoftIn(others.AsEnumerable());

    public static bool NotIn<T>(this T? value, params T[] others) => !value.In((IEnumerable<T>)others);

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(x => x);

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random? random = null)
    {
        random ??= new Random((int)DateTime.UtcNow.Ticks);

        // https://stackoverflow.com/a/1287572/235671
        // https://stackoverflow.com/a/1665080/235671

        var copy = source.ToArray();

        // Iterating backwards. Note i > 0 to avoid final pointless iteration
        for (var i = copy.Length - 1; i > 0; i--)
        {
            // Swap element "i" with a random earlier element it (or itself)
            // ... except we don't really need to swap it fully, as we can
            // return it immediately, and afterwards it's irrelevant.
            var swap = random.Next(i + 1);
            yield return copy[swap];
            copy[swap] = copy[i];
        }

        // there is one item remaining that was not returned - we return it now
        yield return copy[0];
    }

    public static bool IsSubsetOf<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T>? comparer = default)
    {
        comparer ??= EqualityComparer<T>.Default;

        return
            first
                .Intersect(second, comparer)
                .SequenceEqual(first, comparer);
    }

    public static bool IsSupersetOf<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T>? comparer = default)
    {
        return second.IsSubsetOf(first, comparer);
    }

    public static ImmutableDictionary<TKey, TValue> AddWhen<TKey, TValue>(this ImmutableDictionary<TKey, TValue> dictionary, bool condition, TKey key, TValue value) where TKey : notnull
    {
        return
            condition
                ? dictionary.Add(key, value)
                : dictionary;
    }

    public static CollectionDiff<TSource> Diff<TSource, TKey, TValue>
    (
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector,
        IEqualityComparer<TKey> keyComparer,
        IEqualityComparer<TValue> valueComparer
    )
    {
        var both = first.Join(second, keySelector, keySelector, (f, s) => (f, s), keyComparer);

        return new CollectionDiff<TSource>
        {
            Added = second.Except(first, keySelector, keyComparer),
            Removed = first.Except(second, keySelector, keyComparer),
            Same = both.Where(t => valueComparer.Equals(valueSelector(t.f), valueSelector(t.s))).Select(t => t.s),
            Changed = both.Where(t => !valueComparer.Equals(valueSelector(t.f), valueSelector(t.s))).Select(t => t.s)
        };
    }

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

    public static Stack<T> ToStack<T>(this IEnumerable<T> source) => new(source);

    public static IEnumerable<TResult> Merge<TSource, TKey, TResult>
    (
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selectKey,
        Func<IEnumerable<TSource>, TResult> merge
    )
    {
        return
            from x in first.Concat(second)
            group x by selectKey(x) into g
            select merge(g);
    }

    public static IEnumerable<T> MergeFirst<T, TKey>
    (
        this IEnumerable<T> first,
        IEnumerable<T> second,
        Func<T, TKey> selectKey
    )
    {
        return first.Merge(second, selectKey, Enumerable.First);
    }

    public static IEnumerable<T> MergeLast<T, TKey>
    (
        this IEnumerable<T> first,
        IEnumerable<T> second,
        Func<T, TKey> selectKey
    )
    {
        return first.Merge(second, selectKey, Enumerable.Last);
    }

    public static IEnumerable<Match> Match(this IEnumerable<string> source, string pattern, RegexOptions options = RegexOptions.None)
    {
        options = RegexOptions.ExplicitCapture | options;

        foreach (var item in source)
        {
            if (Regex.Match(item, pattern, options) is { Success: true } match)
            {
                yield return match;
            }
        }
    }

    public static IEnumerable<T> Parse<T>(this IEnumerable<Match> source, Func<string, Type, object>? convert = default)
    {
        convert ??= (x, t) =>
        {
            if (t == typeof(int)) return int.Parse(x);
            if (t.IsEnum) return Enum.Parse(t, x);
            return x;
        };

        // It's for records so don't expect any fancy constructors.
        var ctor = typeof(T).GetConstructors().Single();
        var parameters = ctor.GetParameters();

        foreach (var item in source)
        {
            var values =
                from p in parameters
                let g = item.Groups[p.Name!]
                let v = g.Success ? g.Value : throw DynamicException.Create("PropertyNotFound", $"{typeof(T).ToPrettyString()}'s property '{p.Name}' not found.")
                select convert(v, p.ParameterType);

            yield return (T)ctor.Invoke(values.ToArray());
        }
    }

    public static IEnumerable<string> Pad(this IEnumerable<string> values, IEnumerable<int> widths, char padding = ' ')
    {
        foreach (var (value, width) in values.Zip(widths, (x, w) => (Value: x, Width: w)))
        {
            yield return
                width < 0
                    ? value.PadLeft(-width, padding)
                    : value.PadRight(width, padding);
        }
    }
}

public class CollectionDiff<T>
{
    public IEnumerable<T> Added { get; set; } = Enumerable.Empty<T>();
    public IEnumerable<T> Removed { get; set; } = Enumerable.Empty<T>();
    public IEnumerable<T> Same { get; set; } = Enumerable.Empty<T>();
    public IEnumerable<T> Changed { get; set; } = Enumerable.Empty<T>();
}