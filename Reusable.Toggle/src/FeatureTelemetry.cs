using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;

namespace Reusable.FeatureBuzz
{
    [PublicAPI]
    public class FeatureTelemetry : IFeatureCollection
    {
        private readonly IFeatureCollection _features;
        private readonly ILogger<FeatureTelemetry> _logger;

        public FeatureTelemetry(IFeatureCollection features, ILogger<FeatureTelemetry> logger)
        {
            _features = features;
            _logger = logger;
        }

        public Feature this[string name] => _features[name];
        
        public bool TryAdd(Feature feature) => _features.TryAdd(feature);

        public bool TryGet(string name, out Feature? feature) => _features.TryGet(name, out feature);

        public bool TryRemove(string name, out Feature? feature) => _features.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            var feature = this[Feature.Telemetry.CreateName(name)];

            using (_logger.BeginScope("UseFeature", new { feature = feature.Name, policy = feature.Policy.GetType().ToPrettyString(), tags = feature.Tags }))
            {
                try
                {
                    return await _features.Use(name, onEnabled, onDisabled, parameter);
                }
                catch (Exception ex)
                {
                    _logger.Scope().Exceptions.Push(ex);
                }
            }

            return default;
        }
        
        public IEnumerator<Feature> GetEnumerator() => _features.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}