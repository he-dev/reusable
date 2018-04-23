using System.Collections;
using System.Collections.Generic;

namespace Reusable.Sequences
{
    public interface ISequence<out T> : IEnumerable<T> { }

    public abstract class Sequence<T> : ISequence<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
