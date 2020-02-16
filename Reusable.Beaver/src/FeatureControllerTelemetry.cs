using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    public class FeatureControllerTelemetry : IFeatureController
    {
        private readonly IFeatureController _controller;

        private readonly ILogger<FeatureControllerTelemetry> _logger;

        public FeatureControllerTelemetry(IFeatureController controller, ILogger<FeatureControllerTelemetry> logger)
        {
            _controller = controller;
            _logger = logger;
        }

        public Feature this[string name] => _controller[name];
        
        public bool TryGet(string name, out Feature feature) => _controller.TryGet(name, out feature);
        
        public void AddOrUpdate(Feature feature) => _controller.AddOrUpdate(feature);

        public bool TryRemove(string name, out Feature feature) => _controller.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            using (_logger.BeginScope().WithCorrelationHandle("UseFeature").UseStopwatch())
            {
                return await _controller.Use(name, onEnabled, onDisabled, parameter).ContinueWith(t =>
                {
                    if (this.IsEnabled(Feature.Telemetry.CreateName(name)))
                    {
                        var feature = this[name];
                        _logger.Log(Abstraction.Layer.Service().Meta(new
                        {
                            featureTelemetry = new
                            {
                                name = feature.Name,
                                tags = feature.Tags,
                                policy = feature.Policy.GetType().ToPrettyString()
                            }
                        }), log => log.Exception(t.Exception));
                    }

                    return t.Result;
                });
            }
        }

        public IEnumerator<Feature> GetEnumerator() => _controller.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_controller).GetEnumerator();
    }
}