using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle : IFeatureCollection { }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly IFeatureCollection _features;

        public FeatureToggle(IFeaturePolicy fallbackPolicy, IFeatureCollection features)
        {
            _features = features;
            _features.AddOrUpdate(new Feature.Fallback(fallbackPolicy));
        }

        public FeatureToggle(IFeaturePolicy fallbackPolicy) : this(fallbackPolicy, new FeatureCollection()) { }

        /// <summary>
        /// Gets feature or fallback.
        /// </summary>
        public Feature this[string name] => _features[name] ?? _features[nameof(Feature.Fallback)]!;

        /// <summary>
        /// Adds or updates feature. Throws when trying to set a locked feature.
        /// </summary>
        public void AddOrUpdate(Feature feature)
        {
            if (this.IsLocked(feature.Name)) throw new InvalidOperationException($"Feature '{feature.Name}' is locked and cannot be changed.");
            _features.AddOrUpdate(feature);
        }

        public bool TryRemove(string name, out Feature feature) => _features.TryRemove(name, out feature);

        public IEnumerator<Feature> GetEnumerator() => _features.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}