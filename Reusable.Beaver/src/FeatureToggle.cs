using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Beaver.Policies;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle
    {
        IFeaturePolicy this[Feature name] { get; }

        IFeatureToggle AddOrUpdate(IFeaturePolicy policy);

        bool Remove(Feature name);

        bool IsEnabled(Feature name);

        // ReSharper disable once InconsistentNaming - This name is by convention so.
        Task<FeatureActionResult<T>> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default);
    }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly IContainer<Feature, IFeaturePolicy> _policies;

        public FeatureToggle(IContainer<Feature, IFeaturePolicy>? policies = default)
        {
            _policies = policies ?? new FeaturePolicyContainer();
        }

        public IFeaturePolicy this[Feature name] => _policies.GetItem(name).SingleOrDefault() ?? new AlwaysOff(name);

        public IFeatureToggle AddOrUpdate(IFeaturePolicy policy)
        {
            if (this.IsLocked(policy.Feature)) throw new InvalidOperationException($"Feature '{policy.Feature}' is locked and cannot be updated.");

            _policies.AddOrUpdateItem(policy.Feature, policy);
            return this;
        }

        public bool Remove(Feature name)
        {
            if (this.IsLocked(name)) throw new InvalidOperationException($"Feature '{name}' is locked and cannot be removed.");

            return _policies.RemoveItem(name);
        }

        public bool IsEnabled(Feature name)
        {
            return this[name].IsEnabled(new Feature(name)
            {
                Toggle = this
            });
        }

        public async Task<FeatureActionResult<T>> IIf<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default)
        {
            ifDisabled ??= () => Task.FromResult<T>(default);

            // Not catching exceptions because the caller should handle them.

            var context = new Feature(feature)
            {
                Toggle = this,
                Parameter = feature.Parameter,
            };

            var policy = this[feature];

            try
            {
                return
                    policy.IsEnabled(context)
                        ? await InvokeFeature<T, FeatureActionResult<T>.Main>(policy, context, ifEnabled)
                        : await InvokeFeature<T, FeatureActionResult<T>.Fallback>(policy, context, ifDisabled);
            }
            finally
            {
                (policy as IFinalizable)?.FinallyIIf(context);
            }
        }

        private static async Task<FeatureActionResult<T>> InvokeFeature<T, TAction>(IFeaturePolicy policy, Feature feature, Func<Task<T>> body) where TAction : FeatureActionResult<T>, new()
        {
            try
            {
                return new TAction
                {
                    Policy = policy,
                    Value = await body().ConfigureAwait(false)
                };
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"{typeof(TAction).Name}FeatureAction",
                    $"An error occured while trying to use the '{typeof(TAction).Name}' feature action for '{feature}'. See the inner exception for details.",
                    inner
                );
            }
            finally
            {
                if (policy is IFinalizable finalizable)
                {
                    if (typeof(TAction) == typeof(FeatureActionResult<T>.Main))
                    {
                        finalizable.FinallyMain(feature);
                    }

                    if (typeof(TAction) == typeof(FeatureActionResult<T>.Fallback))
                    {
                        finalizable.FinallyFallback(feature);
                    }
                }
            }
        }
    }
}