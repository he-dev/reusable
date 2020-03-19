using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    /// <summary>
    /// This interface extends IFeatureCollection and adds fallback-policy for when a feature does not exist. 
    /// </summary>
    [PublicAPI]
    public interface IFeatureToggle : IFeatureCollection
    {
        /// <summary>
        /// Gets feature or fallback.
        /// </summary>
        Feature this[string name] { get; }
    }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly IFeaturePolicy _fallbackPolicy;
        private readonly IFeatureCollection _features;

        public FeatureToggle(IFeaturePolicy fallbackPolicy, IFeatureCollection features)
        {
            _fallbackPolicy = fallbackPolicy;
            _features = features;
        }

        public FeatureToggle(IFeaturePolicy fallbackPolicy) : this(fallbackPolicy, new FeatureCollection()) { }

        public Feature this[string name]
        {
            get
            {
                TryGet(name, out var feature);
                return feature;
            }
        }

        /// <summary>
        /// Tries to get feature. With FeatureToggle always true because a fallback feature is returned otherwise.
        /// </summary>
        public bool TryGet(string name, out Feature feature)
        {
            if (!_features.TryGet(name, out feature))
            {
                feature = new Feature.Fallback(name, _fallbackPolicy);
            }

            return true;
        }

        public void Add(Feature feature) => _features.Add(feature);

        public bool TryRemove(string name, out Feature feature) => _features.TryRemove(name, out feature);

        public IEnumerator<Feature> GetEnumerator() => _features.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}