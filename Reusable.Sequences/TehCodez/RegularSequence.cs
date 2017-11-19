using System.Collections.Generic;

namespace Reusable.Sequences
{
    public class RegularSequence<T> : Sequence<T>
    {
        private readonly T _value;

        public RegularSequence(T value) => _value = value; 

        public override IEnumerator<T> GetEnumerator()
        {
            while (true) yield return _value;
        }
    }
}
