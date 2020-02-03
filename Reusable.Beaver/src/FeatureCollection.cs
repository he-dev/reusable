using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Beaver
{
    public interface IFeatureCollection : IEnumerable<Feature>
    {
        Feature this[string name] { get; }

        bool TryGet(string name, out Feature feature);

        void AddOrUpdate(Feature feature);

        bool TryRemove(string name, out Feature feature);
    }

    /// <summary>
    /// Stores feature policies. Uses a fallback policy if none was found.
    /// </summary>
    public class FeatureCollection : IFeatureCollection
    {
        private readonly ConcurrentDictionary<string, Feature> _features = new ConcurrentDictionary<string, Feature>(SoftString.Comparer);

        public Feature this[string name] => _features[name]; // .TryGetValue(name, out var feature) ? feature : default;

        public bool TryGet(string name, out Feature feature) => _features.TryGetValue(name, out feature);

        public void AddOrUpdate(Feature feature) => _features.AddOrUpdate(feature.Name, name => feature, (n, f) => feature);

        public bool TryRemove(string name, out Feature feature) => _features.TryRemove(name, out feature);

        public IEnumerator<Feature> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class FeatureCollectionExtensions
    {
        public static void AddOrUpdate(this IFeatureCollection features, string name, IFeaturePolicy policy, params string[] tags)
        {
            features.AddOrUpdate(new Feature(name, policy, tags));
        }
    }
}