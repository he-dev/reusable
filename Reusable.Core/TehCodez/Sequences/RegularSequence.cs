using System.Collections.Generic;

namespace Reusable.Sequences
{
    public class RegularSequence<T> : Sequence<T>
    {
        private readonly T _value;

        public RegularSequence(T value) => _value = value; 

        protected override IEnumerable<T> Generate()
        {
            while (true) yield return _value;
        }
    }
}
