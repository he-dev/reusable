using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Flowingo
{
    public class Any<T> : List<IPredicate<T>>, IPredicate<T>
    {
        public void Add(Func<T, bool> predicate) => Add(new Predicate<T>(predicate));

        public bool Invoke(T context) => this.Any(p => p.Invoke(context));
    }
}