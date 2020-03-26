using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.Beaver
{
    /// <summary>
    /// This class adds telemetry logging to feature-controller.
    /// </summary>
    public class FeatureTelemetry : IFeatureController
    {
        private readonly IFeatureController _controller;

        private readonly ILogger<FeatureTelemetry> _logger;

        public FeatureTelemetry(IFeatureController controller, ILogger<FeatureTelemetry> logger)
        {
            _controller = controller;
            _logger = logger;
        }

        public Feature this[string name] => _controller[name];

        public bool TryGet(string name, out Feature feature) => _controller.TryGet(name, out feature);

        public void Add(Feature feature) => _controller.Add(feature);

        public bool TryRemove(string name, out Feature feature) => _controller.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            var feature = this[name];

            using (_logger.BeginScope("UseFeature", new { feature = feature.Name, policy = feature.Policy.GetType().ToPrettyString(), tags = feature.Tags }))
            {
                try
                {
                    return await _controller.Use(name, onEnabled, onDisabled, parameter);
                }
                catch (Exception ex)
                {
                    _logger.Scope().Exceptions.Push(ex);
                }
            }

            return default;
        }

        public IEnumerator<Feature> GetEnumerator() => _controller.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_controller).GetEnumerator();
    }
}