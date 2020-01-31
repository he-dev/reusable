using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Beaver
{
    public interface IFeatureCollection : IEnumerable<Feature>
    {
        Feature? this[string name] { get; set; }

        void Add(Feature feature);
    }

    /// <summary>
    /// Stores feature policies. Uses a fallback policy if none was found.
    /// </summary>
    public class FeatureCollection : IFeatureCollection
    {
        private readonly ConcurrentDictionary<string, Feature> _features = new ConcurrentDictionary<string, Feature>(SoftString.Comparer);

        public Feature? this[string name]
        {
            get => _features.TryGetValue(name, out var feature) ? feature : default;
            set
            {
                if (value is null)
                {
                    _features.TryRemove(name, out _);
                }
                else
                {
                    _features[name] = value;
                }
            }
        }

        public void Add(Feature feature) => this[feature.Name] = feature;
        
        public IEnumerator<Feature> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}