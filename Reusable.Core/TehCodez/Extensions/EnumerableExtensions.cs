using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Collections;

//namespace Reusable.Extensions
namespace System.Linq.Custom
{
    using Reusable.Extensions;
    using System.Linq;

    [PublicAPI]
    public static class EnumerableExtensions
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
        public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
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

        [NotNull]
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            return enumerable.Concat(new[] { item });
        }

        [NotNull]
        public static IEnumerable<T> Prepend<T>([NotNull] this IEnumerable<T> source, T item)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new[] { item }.Concat(source);
        }

        public static string Join<T>([NotNull] this IEnumerable<T> values, string separator)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return string.Join(separator, values);
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

            return first.Except(second, new ProjectionComparer<TArg, TProjection>(projection));
        }

        public static IEnumerable<string> QuoteAllWith<T>(this IEnumerable<T> values, string quotationMark)
        {
            return values.Select(x => x?.ToString().QuoteWith(quotationMark));
        }

        public static IEnumerable<string> QuoteAllWith<T>(this IEnumerable<T> values, char quotationMark)
        {
            return values.Select(x => x?.ToString().QuoteWith(quotationMark));
        }

        public static IEnumerable<T> LoopTake<T>(this IEnumerable<T> values, int count)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(count),
                    actualValue: count,
                    message: "Count must be greater or equal 0."
                );
            }

            var took = 0;

            // ReSharper disable once PossibleMultipleEnumeration
            var enumerator = values.GetEnumerator();

            try
            {
                while (took < count)
                {
                    if (MoveNext(ref enumerator))
                    {
                        yield return enumerator.Current;
                    }
                    else
                    {
                        yield break;
                    }
                    took++;
                }
            }
            finally
            {
                enumerator.Dispose();
            }

            bool MoveNext(ref IEnumerator<T> e)
            {
                if (e.MoveNext())
                {
                    return true;
                }
                // Could not move-next. Reset enumerator and try again.
                else
                {
                    e.Dispose();

                    // ReSharper disable once PossibleMultipleEnumeration
                    e = values.GetEnumerator();

                    // If we couldn't move after reset then we're done trying.
                    return e.MoveNext();
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

        public static bool None<TSource>([NotNull] this IEnumerable<TSource> source)
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

        public static int CalcHashCode<T>([NotNull, ItemCanBeNull] this IEnumerable<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            unchecked
            {
                return values.Aggregate(0, (current, next) => (current * 397) ^ next?.GetHashCode() ?? 0);
            }
        }
    }

    public class EmptySequenceException : Exception { }
    public class MoreThanOneElementException : Exception { }
}
