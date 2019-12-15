using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Beaver
{
    /// <summary>
    /// Stores feature policies. Uses a fallback policy if none was found.
    /// </summary>
    public class FeaturePolicyContainer : IContainer<Feature, IFeaturePolicy>, IEnumerable<IFeaturePolicy>
    {
        private readonly ConcurrentDictionary<Feature, IFeaturePolicy> policies = new ConcurrentDictionary<Feature, IFeaturePolicy>();
        
        public Maybe<IFeaturePolicy> GetItem(Feature feature)
        {
            return
                policies.TryGetValue(feature, out var policy)
                    ? Maybe.SingleRef(policy, feature)
                    : Maybe<IFeaturePolicy>.Empty(feature);
        }

        public void AddOrUpdateItem(Feature feature, IFeaturePolicy policy)
        {
            policies.AddOrUpdate(feature, name => policy, (f, p) => policy);
        }

        public bool RemoveItem(Feature key)
        {
            return policies.TryRemove(key, out _);
        }

        public IEnumerator<IFeaturePolicy> GetEnumerator() => policies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Collection initializer.
        public void Add(Feature feature, IFeaturePolicy policy) => AddOrUpdateItem(feature, policy);
    }
}