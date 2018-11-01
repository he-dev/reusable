using System.Collections;
using System.Collections.Generic;
using Reusable.Collections.Generators;

namespace Reusable.Collections
{
    public abstract class Sequence<T> : IEnumerable<T>
    {
        #region IEnumerable

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public abstract class Sequence
    {
        public static IEnumerable<T> Constant<T>(T value) => new ConstantSequence<T>(value);

        public static IEnumerable<int> Random(int min, int max) => new RandomSequence(min, max);

        public static IEnumerable<T> Fibonacci<T>(T one) => new FibonacciSequence<T>(one);
    }
}