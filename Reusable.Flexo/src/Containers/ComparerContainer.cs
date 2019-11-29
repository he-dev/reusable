using System.Collections.Generic;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Containers
{
    public class ComparerContainer : Dictionary<SoftString, IComparer<object>>, IContainer<IComparer<object>>
    {
        public Option<IComparer<object>> GetItem(string key)
        {
            return
                TryGetValue(key, out var comparer)
                    ? (comparer, key)
                    : (default, key);
        }
    }
}