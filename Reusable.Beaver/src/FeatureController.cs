using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    /// <summary>
    /// This interface extends IFeatureToggle by providing the Use method.
    /// </summary>
    public interface IFeatureController : IFeatureToggle
    {
        Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default);
    }

    public class FeatureController : IFeatureController
    {
        private readonly IFeatureToggle _toggle;

        public FeatureController(IFeatureToggle toggle) => _toggle = toggle;

        public FeatureController(IFeaturePolicy fallbackPolicy) : this(new FeatureToggle(fallbackPolicy)) { }

        public Feature this[string name] => _toggle[name];

        public void Add(Feature feature) => _toggle.Add(feature);

        public bool TryGet(string name, out Feature feature) => _toggle.TryGet(name, out feature);

        public bool TryRemove(string name, out Feature feature) => _toggle.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            onDisabled ??= () => Task.FromResult(default(T));

            // Not catching exceptions because the caller should handle them.

            var feature = this[name];
            var context = new FeatureContext(this, this[name], parameter);

            try
            {
                var state = feature.Policy.State(context);
                try
                {
                    var action = state switch
                    {
                        FeatureState.Enabled => onEnabled,
                        FeatureState.Disabled => onDisabled,
                        _ => throw new InvalidOperationException($"Feature {feature} must be either {FeatureState.Enabled} or {FeatureState.Disabled}.")
                    };

                    return new FeatureResult<T>
                    {
                        Feature = feature,
                        State = state,
                        Value = await action().ConfigureAwait(false),
                    };
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create
                    (
                        $"FeatureAction",
                        $"An error occured while trying to invoke '{state}' feature action for '{context.Feature}'. See the inner exception for details.",
                        inner
                    );
                }
                finally
                {
                    (feature.Policy as IFinalizable)?.Finalize(context, state);
                }
            }
            finally
            {
                (feature.Policy as IFinalizable)?.Finalize(context, FeatureState.Any);
            }
        }

        public IEnumerator<Feature> GetEnumerator() => _toggle.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_toggle).GetEnumerator();
    }
}