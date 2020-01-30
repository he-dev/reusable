using System;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    public class FeatureTelemetry : IFeatureAgent
    {
        private readonly IFeatureAgent _agent;

        private readonly ILogger<FeatureTelemetry> _logger;

        public FeatureTelemetry(IFeatureAgent agent, ILogger<FeatureTelemetry> logger)
        {
            _agent = agent;
            _logger = logger;
        }

        public IFeatureToggle FeatureToggle => _agent.FeatureToggle;

        public async Task<FeatureResult<T>> Use<T>(string feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default)
        {
            using (_logger.BeginScope().WithCorrelationHandle("UseFeature").UseStopwatch())
            {
                return await _agent.Use(feature, ifEnabled, ifDisabled, parameter).ContinueWith(t =>
                {
                    if (FeatureToggle.IsEnabled($"{feature}.Telemetry"))
                    {
                        _logger.Log(Abstraction.Layer.Service().Meta(new { feature }), log => log.Exception(t.Exception));
                    }

                    return t.Result;
                });
            }
        }
    }
}