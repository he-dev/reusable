using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Flexo.Containers
{
    public class ComparerContainer : Dictionary<SoftString, IComparer<object>>, IContainer<string, IComparer<object>>
    {
        public Maybe<IComparer<object>> GetItem(string key)
        {
            return
                TryGetValue(key, out var comparer)
                    ? (comparer, key)
                    : (default, key);
        }

        public void AddOrUpdateItem(string key, IComparer<object> value)
        {
            base[key] = value;
        }

        public bool RemoveItem(string key)
        {
            return Remove(key);
        }
    }
}