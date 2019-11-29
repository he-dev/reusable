using System.Collections.Generic;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Containers
{
    public class EqualityComparerContainer : Dictionary<SoftString, IEqualityComparer<object>>, IContainer<IEqualityComparer<object>>
    {
        public Maybe<IEqualityComparer<object>> GetItem(string key)
        {
            return
                TryGetValue(key, out var comparer)
                    ? (comparer, key)
                    : (default, key);
        }
    }
}