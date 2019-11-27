using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle
    {
        IFeaturePolicy this[Feature name] { get; }

        IFeatureToggle AddOrUpdate(IFeaturePolicy policy);

        bool Remove(Feature name);

        bool IsEnabled(Feature name, object? parameter = default);

        // ReSharper disable once InconsistentNaming - This name is by convention so.
        Task<FeatureActionResult<T>> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default);
    }

    public class FeatureActionResult<T>
    {
        public IFeaturePolicy Policy { get; set; }
        public T Value { get; set; }
        public override string ToString() => GetType().Name;
        public static implicit operator T(FeatureActionResult<T> telemetry) => telemetry.Value;

        public class Main : FeatureActionResult<T> { }

        public class Fallback : FeatureActionResult<T> { }
    }

    public class FeatureToggle : IFeatureToggle, IEnumerable<IFeaturePolicy>
    {
        private readonly ConcurrentDictionary<Feature, IFeaturePolicy> _policies;

        public FeatureToggle(IDictionary<Feature, IFeaturePolicy> policies = default)
        {
            _policies = new ConcurrentDictionary<Feature, IFeaturePolicy>(policies ?? Enumerable.Empty<KeyValuePair<Feature, IFeaturePolicy>>());
        }

        public IFeaturePolicy this[Feature name] => _policies.TryGetValue(name, out var policy) ? policy : new AlwaysOff(name);

        public IFeatureToggle AddOrUpdate(IFeaturePolicy policy)
        {
            if (this.IsLocked(policy.Feature)) throw new InvalidOperationException($"Feature '{policy.Feature}' is locked and cannot be updated.");

            _policies.AddOrUpdate(policy.Feature, name => policy, (f, p) => policy);
            return this;
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

        public async Task<FeatureActionResult<T>> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default)
        {
            // Not catching exceptions because the caller should handle them.

            var context = new Feature(feature)
            {
                Toggle = this,
                Parameter = feature.Parameter,
            };

            var policy = this[feature];

            try
            {
                if (policy.IsEnabled(context))
                {
                    try
                    {
                        return new FeatureActionResult<T>.Main
                        {
                            Policy = policy,
                            Value = await ifEnabled().ConfigureAwait(false)
                        };
                    }
                    catch (Exception inner)
                    {
                        throw DynamicException.Create("MainFeatureAction", $"An error occured while trying to use the 'Main' feature action for '{feature}'. See the inner exception for details.", inner);
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
                        return new FeatureActionResult<T>.Fallback
                        {
                            Policy = policy,
                            Value = await (ifDisabled ?? (() => Task.FromResult<T>(default)))().ConfigureAwait(false)
                        };
                    }
                    catch (Exception inner)
                    {
                        throw DynamicException.Create("FallbackFeatureAction", $"An error occured while trying to use the 'Fallback' feature action for '{feature}'. See the inner exception for details.", inner);
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
            }
        }


        public IEnumerator<IFeaturePolicy> GetEnumerator() => _policies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}