using System.Collections.Generic;

namespace Reusable.Collections.Generators
{
    public class ConstantSequence<T> : Sequence<T>
    {
        private readonly T _value;

        public ConstantSequence(T value) => _value = value;

        public override IEnumerator<T> GetEnumerator()
        {
            while (true) yield return _value;
            // ReSharper disable once IteratorNeverReturns - this is by design
        }
    }
}