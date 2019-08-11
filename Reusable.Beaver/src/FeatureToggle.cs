using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle
    {
        IFeatureOptionRepository Options { get; }

        Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback);
    }

    public class FeatureToggle : IFeatureToggle
    {
        public FeatureToggle(IFeatureOptionRepository options)
        {
            Options = options;
        }

        public IFeatureOptionRepository Options { get; }

        public async Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            // Not catching exceptions because the caller should handle them.

            return await (this.IsEnabled(name) ? body : fallback)().ConfigureAwait(false);
        }
    }

    public class FeatureToggler : IFeatureToggle
    {
        private readonly IFeatureToggle _featureToggle;

        public FeatureToggler(IFeatureToggle featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public IFeatureOptionRepository Options => _featureToggle.Options;

        public Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            try
            {
                return _featureToggle.ExecuteAsync(name, body, fallback);
            }
            finally
            {
                if (Options[name].Contains(FeatureOption.Toggle))
                {
                    this.Update(name, f => f.Toggle(FeatureOption.Enabled));

                    if (Options[name].Contains(FeatureOption.ToggleOnce))
                    {
                        this.Update(name, f => f.Remove(FeatureOption.Toggle).Remove(FeatureOption.ToggleOnce));

                        if (Options[name].Contains(FeatureOption.ToggleReset))
                        {
                            Options.Remove(name);
                        }

                        Options.SaveChanges(name);
                    }
                }
            }
        }
    }

    public class FeatureTelemetry : IFeatureToggle
    {
        private readonly ILogger _logger;
        private readonly IFeatureToggle _featureToggle;

        public FeatureTelemetry(ILogger<FeatureTelemetry> logger, IFeatureToggle featureToggle)
        {
            _logger = logger;
            _featureToggle = featureToggle;
        }

        public IFeatureOptionRepository Options => _featureToggle.Options;

        public async Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            if (Options[name].Contains(FeatureOption.Telemetry))
            {
                using (_logger.UseScope(correlationHandle: nameof(FeatureTelemetry)))
                using (_logger.UseStopwatch())
                {
                    _logger.Log(Abstraction.Layer.Service().Meta(new {FeatureName = name}).Trace());

                    if (_featureToggle.IsDirty(name))
                    {
                        _logger.Log(Abstraction.Layer.Service().Meta(new {CustomFeatureOptions = _featureToggle.Options[name]}));
                    }

                    return await _featureToggle.ExecuteAsync(name, body, fallback);
                }
            }
            else
            {
                return await _featureToggle.ExecuteAsync(name, body, fallback);
            }
        }
    }
}