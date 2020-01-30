using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Beaver
{
    /// <summary>
    /// Stores feature policies. Uses a fallback policy if none was found.
    /// </summary>
    public class FeaturePolicyContainer : IContainer<string, Feature>, IEnumerable<Feature>
    {
        private readonly ConcurrentDictionary<string, Feature> _features = new ConcurrentDictionary<string, Feature>(SoftString.Comparer);
        
        public Maybe<Feature> GetItem(string feature)
        {
            return
                _features.TryGetValue(feature, out var policy)
                    ? Maybe.FromObject(policy, feature)
                    : Maybe<Feature>.Empty(feature);
        }

        public void AddOrUpdateItem(string name, Feature feature)
        {
            _features.AddOrUpdate(name, n => feature, (f, p) => feature);
        }

        public bool RemoveItem(string name)
        {
            return _features.TryRemove(name, out _);
        }

        public IEnumerator<Feature> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Collection initializer.
        public void Add(string name, Feature feature) => AddOrUpdateItem(name, feature);
    }
}