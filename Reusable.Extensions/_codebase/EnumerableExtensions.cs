using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Extensions
{
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
        public static IEnumerable<TResult> ZipWithDefault<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) { throw new ArgumentNullException(nameof(first)); }
            if (second == null) { throw new ArgumentNullException(nameof(second)); }
            if (resultSelector == null) { throw new ArgumentNullException(nameof(resultSelector)); }

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

        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable == null) { throw new ArgumentNullException(nameof(enumerable)); }

            return enumerable.Concat(new[] { item });
        }

        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
            if (values == null) { throw new ArgumentNullException(nameof(values)); }
            return string.Join(separator, values);
        }
    }
}
