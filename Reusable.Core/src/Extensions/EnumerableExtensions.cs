using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable;
using Reusable.Collections;
using Reusable.Exceptionize;
using Reusable.Extensions;

// ReSharper disable once CheckNamespace
namespace System.Linq.Custom
{
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
            Func<TFirst, TSecond, TResult> resultSelector = default
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

        public static string Join<T>([NotNull] this IEnumerable<T> values, string separator = "")
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return string.Join(separator, values);
        }

        public static string Join<T>([NotNull] this IEnumerable<T> values, Func<T, string> selector, string separator)
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

        [NotNull, ItemCanBeNull, ContractAnnotation("values: null => halt")]
        public static IEnumerable<T> Loop<T>(this IEnumerable<T> values, int startAt = 0)
        {
            if (startAt < 0)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(startAt), message: $"{nameof(startAt)} must be >= 0");
            }

            var moves = 0;

            // ReSharper disable once PossibleMultipleEnumeration
            var enumerator = values.GetEnumerator();

            try
            {
                while (TryMoveNext(enumerator, out enumerator))
                {
                    moves++;

                    if (startAt > 0 && moves <= startAt)
                    {
                        continue;
                    }

                    yield return enumerator.Current;
                }
            }
            finally
            {
                enumerator.Dispose();
            }

            bool TryMoveNext(IEnumerator<T> currentEnumerator, out IEnumerator<T> newEnumerator)
            {
                if (currentEnumerator.MoveNext())
                {
                    newEnumerator = currentEnumerator;
                    return true;
                }
                else
                {
                    // Get a new enumerator because we took all elements and try again.

                    currentEnumerator.Dispose();

                    // ReSharper disable once PossibleMultipleEnumeration
                    newEnumerator = values.GetEnumerator();

                    // If we couldn't move after reset then we're done trying because the collection is empty.
                    return newEnumerator.MoveNext();
                }
            }
        }

        /// <summary>
        /// Determines if the first collection starts with the second collection.
        /// </summary>
        public static bool StartsWith<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, [NotNull] IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e2.MoveNext())
                {
                    if (!(e1.MoveNext() && comparer.Equals(e1.Current, e2.Current))) return false;
                }

                if (e2.MoveNext()) return false;
            }

            return true;
        }

        public static bool StartsWith<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return first.StartsWith(second, EqualityComparer<TSource>.Default);
        }

        public static bool Empty<TSource>([NotNull] this IEnumerable<TSource> source)
        {
            return source.Any() == false;
        }

        public static bool EmptyOr<TSource>(this IEnumerable<TSource> source, Predicate<IEnumerable<TSource>> predicate)
        {
            return
                source.Any()
                    ? predicate(source)
                    : true;
        }

        public static IEnumerable<T> Skip<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<T, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return source.Where(x => !predicate(x));
        }

        [NotNull]
        public static Func<TElement, bool> ToAny<TElement>([NotNull] this IEnumerable<Func<TElement, bool>> filters)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));

            return x => filters.Any(f => f(x));
        }

        public static T SingleOrThrow<T>([NotNull] this IEnumerable<T> source, Func<Exception>? onEmpty = null, Func<Exception>? onMany = null)
        {
            //return source.SingleOrThrow(_ => true, onEmpty, onMultiple);

            if (source == null) throw new ArgumentNullException(nameof(source));

            var items = source.Take(2).ToList();

            onEmpty ??= () => DynamicException.Create("Empty", $"{source.GetType().ToPrettyString()} does not contain any elements.");
            onMany ??= () => DynamicException.Create("Many", $"{source.GetType().ToPrettyString()} contains more than one element.");

            return items.Count switch
            {
                0 => throw onEmpty(),
                1 => items[0],
                _ => throw onMany()
            };
        }

        public static T SingleOrThrow<T>([NotNull] this IEnumerable<T> source, (string Name, string Message) onEmpty = default, (string Name, string Message) onMany = default)
        {
            return source.SingleOrThrow
            (
                onEmpty: () => DynamicException.Create(onEmpty.Name ?? "Empty", onEmpty.Message ?? $"{source.GetType().ToPrettyString()} does not contain any elements."),
                onMany: () => DynamicException.Create(onMany.Name ?? "Many", onMany.Message ?? $"{source.GetType().ToPrettyString()} contains more than one element.")
            );
        }

        public static T FirstOrThrow<T>(this IEnumerable<T> source, (string Exception, string Message) onEmpty = default)
        {
            return source.Take(1).ToList() switch
            {
                {} items when items.Any() => items.Single(),
                _ => throw DynamicException.Create(onEmpty.Exception ?? "Empty", onEmpty.Message ?? $"{source.GetType().ToPrettyString()} does not contain any elements.")
            };
        }

//        public static T SingleOrThrow<T>([NotNull] this IEnumerable<T> source, Func<T, bool> predicate, Func<Exception> onEmpty = null, Func<Exception> onMany = null)
//        {
//            if (source == null) throw new ArgumentNullException(nameof(source));
//
//            var items = source.Where(predicate).Take(2).ToList();
//
//            onEmpty = onEmpty ?? (() => DynamicException.Create("Empty", $"{source.GetType().ToPrettyString()} does not contain any elements that match the specified predicate."));
//            onMany = onMany ?? (() => DynamicException.Create("Many", $"{source.GetType().ToPrettyString()} contains more than one element that matches the specified predicate."));
//
//            switch (items.Count)
//            {
//                case 0: throw onEmpty();
//                case 1: return items[0];
//                default: throw onMany();
//            }
//        }

        public static int CalcHashCode<T>([NotNull, ItemCanBeNull] this IEnumerable<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            unchecked
            {
                return values.Aggregate(0, (current, next) => (current * 397) ^ next?.GetHashCode() ?? 0);
            }
        }

        [NotNull, ItemCanBeNull]
        public static IEnumerable<T> Repeat<T>(T item)
        {
            while (true)
            {
                yield return item;
            }

            // ReSharper disable once IteratorNeverReturns - Since it's 'Always' this is by design.
        }

        public static IEnumerable<T> Repeat<T>([NotNull] Func<T> get)
        {
            if (get == null) throw new ArgumentNullException(nameof(get));

            while (true)
            {
                yield return get();
            }
        }

        public static bool In<T>([CanBeNull] this T value, [NotNull] IEnumerable<T> others, [NotNull] IEqualityComparer<T> comparer)
        {
            return others.Contains(value, comparer);
        }

        public static bool In<T>([CanBeNull] this T value, [NotNull] IEnumerable<T> others)
        {
            return value.In(others, EqualityComparer<T>.Default);
        }

        public static bool In<T>([CanBeNull] this T value, params T[] others)
        {
            return value.In(others.AsEnumerable());
        }

        public static bool SoftIn([CanBeNull] this string value, IEnumerable<string> others)
        {
            return value.In(others, SoftString.Comparer);
        }

        public static bool SoftIn([CanBeNull] this string value, params string[] others) => value.SoftIn(others.AsEnumerable());

        public static bool NotIn<T>([CanBeNull] this T value, params T[] others)
        {
            return !value.In((IEnumerable<T>)others);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(x => x);

        [NotNull, ItemCanBeNull]
        public static IEnumerable<T> Shuffle<T>([ItemCanBeNull] this IEnumerable<T> source, Random? random = null)
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

        public static ImmutableDictionary<TKey, TValue> AddWhen<TKey, TValue>(this ImmutableDictionary<TKey, TValue> dictionary, bool condition, TKey key, TValue value)
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

        public static Stack<T> ToStack<T>(this IEnumerable<T> source) => new Stack<T>(source);

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




        //        public static IEnumerable<T> Take<T>(this IEnumerable<T> source, int count)
//        {
//            if (source == null) throw new ArgumentNullException(nameof(source));
//            if (count <= 0) return source;
//
//            if (count >= 0)
//            {
//                return Enumerable.Take(source, count);
//            }
//
//            if (source is ICollection<T> collection)
//            {
//                return Enumerable.Take(collection, collection.Count - count);
//            }
//
//            return Skipper();
//
//            IEnumerable<T> Skipper()
//            {
//                using (var e = source.GetEnumerator())
//                {
//                    if (!e.MoveNext())
//                    {
//                        yield break;
//                    }
//
//                    var queue = new T[count];
//                    queue[0] = e.Current;
//                    var index = 1;
//
//                    while (index < count && e.MoveNext())
//                    {
//                        queue[index++] = e.Current;
//                    }
//
//                    index = -1;
//                    while (e.MoveNext())
//                    {
//                        index = (index + 1) % count;
//                        yield return queue[index];
//                        queue[index] = e.Current;
//                    }
//                }
//            }
//        }
    }

    public class CollectionDiff<T>
    {
        public IEnumerable<T> Added { get; set; } = Enumerable.Empty<T>();
        public IEnumerable<T> Removed { get; set; } = Enumerable.Empty<T>();
        public IEnumerable<T> Same { get; set; } = Enumerable.Empty<T>();
        public IEnumerable<T> Changed { get; set; } = Enumerable.Empty<T>();
    }
}