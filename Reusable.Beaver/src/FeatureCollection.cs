using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureCollection : IEnumerable<Feature>
    {
        /// <summary>
        /// Adds feature and throws when feature already added.
        /// </summary>
        void Add(Feature feature);

        bool TryGet(string name, out Feature feature);

        bool TryRemove(string name, out Feature feature);
    }

    /// <summary>
    /// Stores feature policies. Uses a fallback policy if none was found.
    /// </summary>
    [PublicAPI]
    public class FeatureCollection : IFeatureCollection
    {
        private readonly ConcurrentDictionary<string, Feature> _features;

        public FeatureCollection(IEnumerable<Feature> features)
        {
            _features = new ConcurrentDictionary<string, Feature>(features.Select(f => new KeyValuePair<string, Feature>(f.Name, f)), SoftString.Comparer);
        }

        public FeatureCollection(IEnumerable<KeyValuePair<string, bool>> features)
            : this(features.Select(x => new Feature(x.Key, x.Value ? FeaturePolicy.AlwaysOn : FeaturePolicy.AlwaysOff))) { }

        public FeatureCollection()
            : this(Enumerable.Empty<Feature>()) { }

        public void Add(Feature feature)
        {
            if (_features.ContainsKey(feature))
            {
                throw new InvalidOperationException($"Feature {feature} is already added.");
            }

            _features.AddOrUpdate(feature.Name, name => feature, (n, f) => feature);
        }

        public bool TryGet(string name, out Feature feature) => _features.TryGetValue(name, out feature);

        public bool TryRemove(string name, out Feature feature) => _features.TryRemove(name, out feature);

        public IEnumerator<Feature> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}