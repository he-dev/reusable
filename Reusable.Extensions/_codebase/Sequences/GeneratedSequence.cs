using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Sequences
{
    public abstract class GeneratedSequence<T> : IEnumerable<T>
    {
        protected GeneratedSequence(int count) { Count = count; }

        public int Count { get; }

        public IEnumerator<T> GetEnumerator() => Generate().Take(Count).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract IEnumerable<T> Generate();
    }
}
