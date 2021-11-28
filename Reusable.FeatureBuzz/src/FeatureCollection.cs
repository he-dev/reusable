using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.FeatureBuzz
{
    [PublicAPI]
    public interface IFeatureCollection : IEnumerable<Feature>
    {
        /// <summary>
        /// Gets feature or fallback.
        /// </summary>
        Feature this[string name] { get; }

        /// <summary>
        /// Adds feature and throws when feature already added.
        /// </summary>
        bool TryAdd(Feature feature);

        bool TryGet(string name, out Feature? feature);

        bool TryRemove(string name, out Feature? feature);

        Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default);
    }

    /// <summary>
    /// Stores feature policies. Uses a fallback policy if none was found.
    /// </summary>
    [PublicAPI]
    public class FeatureCollection : IFeatureCollection
    {
        private readonly ConcurrentDictionary<string, Feature> _features;
        private readonly IFeaturePolicy _defaultPolicy;

        public FeatureCollection(IImmutableSet<Feature> features, IFeaturePolicy? defaultPolicy = default)
        {
            _defaultPolicy = defaultPolicy ?? FeaturePolicy.Disabled;
            _features = new ConcurrentDictionary<string, Feature>(features.Select(f => new KeyValuePair<string, Feature>(f.Name, f)), SoftString.Comparer);
        }

        public FeatureCollection(IDictionary<string, bool> features, IFeaturePolicy? defaultPolicy = default)
            : this(Helpers.FromDictionary(features), defaultPolicy) { }

        public FeatureCollection()
            : this(ImmutableHashSet<Feature>.Empty) { }

        public Feature this[string name]
        {
            get
            {
                return
                    _features.TryGetValue(name, out var feature)
                        ? feature
                        : new Feature.Fallback(name, _defaultPolicy).Also(f => TryAdd(f));
            }
        }

        public bool TryAdd(Feature feature) => _features.TryAdd(feature.Name, feature);

        public bool TryGet(string name, out Feature? feature) => _features.TryGetValue(name, out feature);

        public bool TryRemove(string name, out Feature? feature) => _features.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            onDisabled ??= () => Task.FromResult(default(T));

            // Not catching exceptions because the caller should handle them.

            var feature = this[name];
            //var context = new FeatureContext { Features = this, Feature = this[name], Parameter = parameter };
            var state = feature.Policy.State(this, name, parameter);

            try
            {
                var action = state switch
                {
                    FeatureState.Enabled => onEnabled,
                    FeatureState.Disabled => onDisabled,
                    _ => throw new InvalidOperationException($"Feature {feature} must be either {FeatureState.Enabled} or {FeatureState.Disabled}.")
                };

                return new FeatureResult<T>
                {
                    Feature = feature,
                    Value = await action().ConfigureAwait(false),
                };
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("Feature", $"Could not use {state} feature '{feature}'. See the inner exception for details.", inner);
            }
            finally
            {
                (feature.Policy as IFeaturePolicyFilter)?.OnFeatureUsed(this, name, parameter);
            }
        }

        public IEnumerator<Feature> GetEnumerator() => _features.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static class Helpers
        {
            public static IImmutableSet<Feature> FromDictionary(IDictionary<string, bool> features)
            {
                return features.Select(x => new Feature(x.Key, x.Value ? FeaturePolicy.Enabled : FeaturePolicy.Disabled)).ToImmutableHashSet();
            }
        }
    }
}