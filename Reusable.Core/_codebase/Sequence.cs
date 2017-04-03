using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
    public abstract class Sequence<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => Generate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract IEnumerable<T> Generate();
    }
}
