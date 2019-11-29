using System.Collections.Generic;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Containers
{
    public class ComparerContainer : Dictionary<SoftString, IComparer<object>>, IContainer<IComparer<object>>
    {
        public Maybe<IComparer<object>> GetItem(string key)
        {
            return
                TryGetValue(key, out var comparer)
                    ? (comparer, true, $"{nameof(ComparerContainer)}.{key}")
                    : (default, false, $"{nameof(ComparerContainer)}.{key}");
        }
    }
}