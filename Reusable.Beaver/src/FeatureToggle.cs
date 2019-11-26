using System;
using System.Collections;
using System.Collections.Generic;
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

        IDictionary<FeatureIdentifier, IFeauturePolicy> Policies { get; }

        // ReSharper disable once InconsistentNaming - This name is by convention so.
        Task<T> IIf<T>(FeatureIdentifier name, Func<Task<T>> ifEnabled, Func<Task<T>> ifDisabled);
    }

    public class FeatureToggle : IFeatureToggle
    {
        public FeatureToggle(IFeatureOptionRepository options)
        {
            Options = options;
        }

        public IFeatureOptionRepository Options { get; }

        public IDictionary<FeatureIdentifier, IFeauturePolicy> Policies { get; } = new Dictionary<FeatureIdentifier, IFeauturePolicy>();

        public Task<T> IIf<T>(FeatureIdentifier name, Func<Task<T>> ifEnabled, Func<Task<T>> ifDisabled)
        {
            // Not catching exceptions because the caller should handle them.

            if (Policies.TryGetValue(name, out var policy))
            {
                try
                {
                    return policy.IsEnabled(name, TODO) ? ifEnabled() : ifDisabled();
                }
                finally
                {
                    policy.Finally(name, this);
                }
            }
            else
            {
                return ifDisabled();
            }
        }
    }

    public interface IFeauturePolicy
    {
        bool IsEnabled(FeatureIdentifier name, IFeatureToggle featureToggle, object context = default);

        void Finally(FeatureIdentifier name, IFeatureToggle featureToggle);
    }

    public class Flag : IFeauturePolicy
    {
        public Flag(bool value) => Value = value;

        public bool Value { get; }

        public bool IsEnabled(FeatureIdentifier name, IFeatureToggle featureToggle, object context = default) => Value;

        public void Finally(FeatureIdentifier name, IFeatureToggle featureToggle) { }
    }
    
    public class Once : IFeauturePolicy
    {
        public bool IsEnabled(FeatureIdentifier name, IFeatureToggle featureToggle, object context = default) => true;

        public void Finally(FeatureIdentifier name, IFeatureToggle featureToggle) => featureToggle.Policies.Remove(name);
    }

    public class FeatureToggler : IFeatureToggle
    {
        private readonly IFeatureToggle _featureToggle;

        public FeatureToggler(IFeatureToggle featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public IFeatureOptionRepository Options => _featureToggle.Options;

        public Task<T> IIf<T>(FeatureIdentifier name, Func<Task<T>> ifEnabled, Func<Task<T>> ifDisabled)
        {
            try
            {
                return _featureToggle.IIf(name, ifEnabled, ifDisabled);
            }
            finally
            {
                if (Options[name].Contains(Feature.Options.Toggle))
                {
                    this.Update(name, f => f.Toggle(Feature.Options.Enabled));

                    if (Options[name].Contains(Feature.Options.ToggleOnce))
                    {
                        this.Update(name, f => f.Remove(Feature.Options.Toggle).Remove(Feature.Options.ToggleOnce));

                        if (Options[name].Contains(Feature.Options.ToggleReset))
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

        public async Task<T> IIf<T>(FeatureIdentifier name, Func<Task<T>> ifEnabled, Func<Task<T>> ifDisabled)
        {
            if (Options[name].Contains(Feature.Options.Telemetry))
            {
                using (_logger.UseScope().WithCorrelationHandle("CollectFeatureTelemetry").UseStopwatch())
                {
                    _logger.Log(Abstraction.Layer.Service().Meta(new
                    {
                        Name = name.ToString(),
                        Options = _featureToggle.Options[name].ToString(),
                        IsDirty = _featureToggle.IsDirty(name)
                    }, nameof(FeatureTelemetry)).Trace());

                    return await _featureToggle.IIf(name, ifEnabled, ifDisabled);
                }
            }
            else
            {
                return await _featureToggle.IIf(name, ifEnabled, ifDisabled);
            }
        }
    }
}