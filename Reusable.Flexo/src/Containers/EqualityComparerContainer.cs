using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Flexo.Containers
{
    public class EqualityComparerContainer : Dictionary<SoftString, IEqualityComparer<object>>, IContainer<string, IEqualityComparer<object>>
    {
        public Maybe<IEqualityComparer<object>> GetItem(string key)
        {
            return
                TryGetValue(key, out var comparer)
                    ? Maybe.FromObject(comparer, key)
                    : Maybe<IEqualityComparer<object>>.Empty(key);
        }

        public void AddOrUpdateItem(string key, IEqualityComparer<object> value)
        {
            base[key] = value;
        }
        
        public bool RemoveItem(string key)
        {
            return Remove(key);
        }
    }
}