using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle
    {
        IFeaturePolicy this[Feature name] { get; }

        void AddOrUpdate(IFeaturePolicy policy);

        bool Remove(Feature name);

        bool IsEnabled(Feature name, object? parameter = default);

        // ReSharper disable once InconsistentNaming - This name is by convention so.
        Task<T> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default);
    }

    public class FeatureToggle : IFeatureToggle, IEnumerable<IFeaturePolicy>
    {
        private readonly ILogger<FeatureToggle> _logger;
        private readonly ConcurrentDictionary<Feature, IFeaturePolicy> _policies;

        public FeatureToggle(ILogger<FeatureToggle> logger, IDictionary<Feature, IFeaturePolicy>? policies = default)
        {
            _logger = logger;
            _policies = new ConcurrentDictionary<Feature, IFeaturePolicy>(policies ?? Enumerable.Empty<KeyValuePair<Feature, IFeaturePolicy>>());
        }

        public IFeaturePolicy this[Feature name] => _policies.TryGetValue(name, out var policy) ? policy : new AlwaysOff(name);

        public void AddOrUpdate(IFeaturePolicy policy)
        {
            if (this.IsLocked(policy.Name)) throw new InvalidOperationException($"Feature '{policy.Name}' is locked and cannot be updated.");

            _policies.AddOrUpdate(policy.Name, name => policy, (f, p) => policy);
        }

        public bool Remove(Feature name)
        {
            if (this.IsLocked(name)) throw new InvalidOperationException($"Feature '{name}' is locked and cannot be removed.");

            return _policies.TryRemove(name, out _);
        }

        public bool IsEnabled(Feature name, object? parameter = default)
        {
            return _policies.TryGetValue(name, out var policy) && policy.IsEnabled(new Feature(name)
            {
                Toggle = this,
                Parameter = parameter
            });
        }

        public async Task<T> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default)
        {
            // Not catching exceptions because the caller should handle them.

            var context = new Feature(feature)
            {
                Toggle = this,
                Parameter = feature.Parameter,
            };

            var policy = this[feature];

            using var scope = _logger.BeginScope().WithCorrelationHandle($"CollectFeatureTelemetry").UseStopwatch();
            try
            {
                var isEnabled = policy.IsEnabled(context);
                _logger.Log(Abstraction.Layer.Service().Subject(new { Feature = new { feature.Name, isEnabled, type = policy.GetType().ToPrettyString() } }));

                if (isEnabled)
                {
                    try
                    {
                        return await ifEnabled().ConfigureAwait(false);
                    }
                    finally
                    {
                        (policy as IFinalizable)?.FinallyMain(context);
                    }
                }
                else
                {
                    try
                    {
                        return await (ifDisabled ?? (() => Task.FromResult<T>(default)))().ConfigureAwait(false);
                    }
                    finally
                    {
                        (policy as IFinalizable)?.FinallyFallback(context);
                    }
                }
            }
            finally
            {
                (policy as IFinalizable)?.FinallyIIf(context);
                _logger.Log(Abstraction.Layer.Service().Routine(nameof(IIf)).Completed());
            }
        }


        public IEnumerator<IFeaturePolicy> GetEnumerator() => _policies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}