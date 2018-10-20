using System.Collections;
using System.Collections.Generic;

namespace Reusable.Collections.Generators
{
    public abstract class Sequence<T> : IEnumerable<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}