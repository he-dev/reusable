using System.Collections.Generic;

namespace Reusable.Sequences
{
    public class RegularSequence<T> : GeneratedSequence<T>
    {
        private readonly T _value;
        public RegularSequence(T value, int count) : base(count) { _value = value; }
        protected override IEnumerable<T> Generate()
        {
            while (true) yield return _value;
        }
    }
}
