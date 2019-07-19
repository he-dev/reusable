using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
        /// <returns>An IEnumerable<T> that contains elements of the two input sequences, combined by resultSelector.</returns>
        public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>
        (
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector
        )
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException(nameof(resultSelector));
            }

            using (var enumeratorFirst = first.GetEnumerator())
            using (var enumeratorSecond = second.GetEnumerator())
            {
                var isEndOfFirst = !enumeratorFirst.MoveNext();
                var isEndOfSecond = !enumeratorSecond.MoveNext();
                while (!isEndOfFirst || !isEndOfSecond)
                {
                    yield return resultSelector(
                        isEndOfFirst ? default(TFirst) : enumeratorFirst.Current,
                        isEndOfSecond ? default(TSecond) : enumeratorSecond.Current);

                    isEndOfFirst = !enumeratorFirst.MoveNext();
                    isEndOfSecond = !enumeratorSecond.MoveNext();
                }
            }
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

        public static string Join<T>([NotNull] this IEnumerable<T> values, string separator)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return string.Join(separator, values);
        }

        public static string Join<T>([NotNull] this IEnumerable<T> values, Func<T, string> selector, string separator)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return string.Join(separator, values.Select(selector));
        }

        public static IEnumerable<TArg> Except<TArg, TProjection>(this IEnumerable<TArg> first, IEnumerable<TArg> second, Func<TArg, TProjection> projection)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null) throw new ArgumentNullException(nameof(second));
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            if (ReferenceEquals(first, projection))
            {
                throw new ArgumentException(paramName: nameof(projection), message: "Projection must be an anonymous type.");
            }

            return first.Except(second, ProjectionEqualityComparer<TArg>.Create(projection));
        }

        public static IEnumerable<string> QuoteAllWith<T>(this IEnumerable<T> values, string quotationMark)
        {
            return values.Select(x => x?.ToString().QuoteWith(quotationMark));
        }

        public static IEnumerable<string> QuoteAllWith<T>(this IEnumerable<T> values, char quotationMark)
        {
            return values.Select(x => x?.ToString().QuoteWith(quotationMark));
        }

        [NotNull, ItemCanBeNull, ContractAnnotation("values: null => halt")]
        public static IEnumerable<T> Loop<T>(this IEnumerable<T> values, int startAt = 0)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

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
        /// <typeparam name="TSource"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
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
            if (source == null) throw new ArgumentNullException(nameof(source));

            return !source.Any();
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

        /// <summary>
        /// Unlike the original Single this method throws two different exceptions: EmptySequenceException or MoreThanOneElementException.
        /// </summary>
        [CanBeNull]
        public static T Single2<T>([NotNull] this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext() == false)
                {
                    throw new EmptySequenceException();
                }

                var single = enumerator.Current;
                if (enumerator.MoveNext())
                {
                    throw new MoreThanOneElementException();
                }

                return single;
            }
        }

        [CanBeNull]
        public static T SingleOrThrow<T>([NotNull] this IEnumerable<T> source, Func<Exception> onEmpty = null, Func<Exception> onMultiple = null)
        {
            return source.SingleOrThrow(_ => true, onEmpty, onMultiple);
        }

        [CanBeNull]
        public static T SingleOrThrow<T>([NotNull] this IEnumerable<T> source, Func<T, bool> predicate, Func<Exception> onEmpty = null, Func<Exception> onMultiple = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = default(T);
            var count = 0;

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        if (++count > 1)
                        {
                            throw onMultiple?.Invoke() ?? DynamicException.Create
                                  (
                                      $"{source.GetType().ToPrettyString()}ContainsMoreThanOneElement",
                                      $"There is more than one element that matches the specified predicate."
                                  );
                        }

                        result = enumerator.Current;
                    }
                }
            }

            if (count == 0)
            {
                throw onEmpty?.Invoke() ?? DynamicException.Create
                      (
                          $"{source.GetType().ToPrettyString()}Empty",
                          $"There is no element that match the specified predicate."
                      );
            }

            return result;
        }

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

        public static bool In<T>([CanBeNull] this T value, params T[] others)
        {
            return value.In((IEnumerable<T>)others);
        }

        public static bool NotIn<T>([CanBeNull] this T value, params T[] others)
        {
            return !value.In((IEnumerable<T>)others);
        }

        public static bool In<T>([CanBeNull] this T value, [NotNull] IEnumerable<T> others)
        {
            if (others == null) throw new ArgumentNullException(nameof(others));

            return value.In(others, EqualityComparer<T>.Default);
        }

        public static bool In<T>([CanBeNull] this T value, [NotNull] IEnumerable<T> others, [NotNull] IEqualityComparer<T> comparer)
        {
            if (others == null) throw new ArgumentNullException(nameof(others));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return others.Contains(value, comparer);
        }

        [NotNull, ItemCanBeNull]
        public static IEnumerable<T> Shuffle<T>([NotNull, ItemCanBeNull] this IEnumerable<T> source, [CanBeNull] Random random = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            random = random ?? new Random((int)DateTime.UtcNow.Ticks);

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

        public static bool IsSubsetOf<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer = default)
        {
            return
                !second
                    .Except(first, comparer ?? EqualityComparer<T>.Default)
                    .Any();
        }

        public static ImmutableDictionary<TKey, TValue> AddWhen<TKey, TValue>(this ImmutableDictionary<TKey, TValue> dictionary, bool condition, TKey key, TValue value)
        {
            return
                condition
                    ? dictionary.Add(key, value)
                    : dictionary;
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

    public class EmptySequenceException : Exception
    {
        public EmptySequenceException() : base("Sequence does not contain any elements.") { }
    }

    public class MoreThanOneElementException : Exception
    {
        public MoreThanOneElementException() : base("Sequence contains more then one element.") { }
    }
}