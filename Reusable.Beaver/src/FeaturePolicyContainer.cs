using System.Collections.Concurrent;
using Reusable.Data;

namespace Reusable.Beaver
{
    public class FeaturePolicyContainer : IContainer<Feature, IFeaturePolicy>
    {
        private readonly ConcurrentDictionary<Feature, IFeaturePolicy> _policies = new ConcurrentDictionary<Feature, IFeaturePolicy>();

        public Maybe<IFeaturePolicy> GetItem(Feature feature)
        {
            return
                _policies.TryGetValue(feature, out var policy)
                    ? (policy, feature)
                    : (default, feature);
        }

        public void AddOrUpdateItem(Feature feature, IFeaturePolicy policy)
        {
            _policies.AddOrUpdate(feature, name => policy, (f, p) => policy);
        }

        public bool RemoveItem(Feature key)
        {
            return _policies.TryRemove(key, out _);
        }
    }
}