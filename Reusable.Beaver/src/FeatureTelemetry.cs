using System;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    public class FeatureTelemetry : IFeatureToggle
    {
        private readonly ILogger<FeatureToggle> _logger;
        private readonly IFeatureToggle _toggle;

        public FeatureTelemetry(ILogger<FeatureToggle> logger, IFeatureToggle toggle)
        {
            _logger = logger;
            _toggle = toggle;
        }

        public IFeaturePolicy this[Feature name] => _toggle[name];

        public IFeatureToggle AddOrUpdate(IFeaturePolicy policy) => _toggle.AddOrUpdate(policy);

        public bool Remove(Feature name) => _toggle.Remove(name);

        public bool IsEnabled(Feature name, object parameter = default) => _toggle.IsEnabled(name, parameter);

        public async Task<FeatureActionResult<T>> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>> ifDisabled = default)
        {
            using (_logger.BeginScope().WithCorrelationHandle("CollectFeatureTelemetry").UseStopwatch())
            {
                _logger.Log(Abstraction.Layer.Service().Subject(new { Feature = feature.Name }).Trace());

                return (await _toggle.IIf(feature, ifEnabled, ifDisabled)).Pipe(r => _logger.Log(Abstraction.Layer.Service().Meta(new
                {
                    FeatureUsageInfo = new
                    {
                        action = r.ToString(),
                        policy = r.Policy.GetType().ToPrettyString()
                    }
                }).Trace()));
            }
        }
    }
}