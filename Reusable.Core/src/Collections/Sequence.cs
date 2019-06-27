using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Collections.Generators;

namespace Reusable.Collections
{
    public abstract class Sequence<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _value;

        protected Sequence(IEnumerable<T> value) => _value = value;

        #region IEnumerable

        public virtual IEnumerator<T> GetEnumerator() => _value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public abstract class Sequence
    {
        /// <summary>
        /// Creates a constant sequences for which all terms are the same.
        /// </summary>
        public static IEnumerable<T> Constant<T>(T value) => new ConstantSequence<T>(value);

        public static IEnumerable<int> Random(int min, int max) => new RandomSequence(min, max);

        public static IEnumerable<T> Fibonacci<T>(T one) => new FibonacciSequence<T>(one);

        public static IEnumerable<T> Monotonic<T>(T start, T step) => new MonotonicSequence<T>(start, step);

        public static IEnumerable<T> InfiniteDefault<T>()
        {
            using (var e = new InfiniteDefaultEnumerator<T>())
            {
                while (e.MoveNext())
                {
                    yield return default;
                }
            }
        }

        public static IEnumerable<T> Custom<T>(T first, Func<T, T> next)
        {
            yield return first;
            var previous = first;
            foreach (var _ in InfiniteDefault<T>())
            {
                yield return previous = next(previous);
            }
        }
    }

    public class InfiniteDefaultEnumerator<T> : IEnumerator<T>
    {
        public T Current => default;

        object IEnumerator.Current => Current;

        public bool MoveNext() => true;

        public void Reset() { }

        public void Dispose() { }
    }
}