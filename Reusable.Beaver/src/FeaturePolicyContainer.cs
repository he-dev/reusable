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
        private readonly ConcurrentDictionary<Feature, IFeaturePolicy> _policies;

        public FeaturePolicyContainer(IFeaturePolicy fallback)
        {
            _policies = new ConcurrentDictionary<Feature, IFeaturePolicy>
            {
                [FeaturePolicy.Fallback] = fallback
            };
        }

        public Maybe<IFeaturePolicy> GetItem(Feature feature)
        {
            return
                _policies.TryGetValue(feature, out var policy)
                    ? Maybe.SingleRef(policy, feature)
                    : Maybe.SingleRef(_policies[FeaturePolicy.Fallback], feature);
        }

        public void AddOrUpdateItem(Feature feature, IFeaturePolicy policy)
        {
            _policies.AddOrUpdate(feature, name => policy, (f, p) => policy);
        }

        public bool RemoveItem(Feature key)
        {
            return _policies.TryRemove(key, out _);
        }

        public IEnumerator<IFeaturePolicy> GetEnumerator() => _policies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Collection initializer.
        public void Add(Feature feature, IFeaturePolicy policy) => AddOrUpdateItem(feature, policy);
    }
}