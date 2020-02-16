using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    public interface IFeatureController : IFeatureToggle
    {
        Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default);
    }

    public class FeatureController : IFeatureController
    {
        private readonly IFeatureToggle _toggle;

        public FeatureController(IFeatureToggle toggle)
        {
            _toggle = toggle;
        }

        public Feature this[string name] => _toggle[name];

        public bool TryGet(string name, out Feature feature) => _toggle.TryGet(name, out feature);
        
        public void AddOrUpdate(Feature feature) => _toggle.AddOrUpdate(feature);

        public bool TryRemove(string name, out Feature feature) => _toggle.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            onDisabled ??= () => Task.FromResult<T>(default);

            // Not catching exceptions because the caller should handle them.

            var context = new FeatureContext(this, name, parameter);
            var feature = this[name];

            try
            {
                var state = feature.Policy.State(context);
                try
                {
                    var result = await (state == FeatureState.Enabled ? onEnabled : onDisabled)().ConfigureAwait(false);
                    return new FeatureResult<T>
                    {
                        Value = result,
                        Feature = feature,
                        State = state,
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
                    (feature.Policy as IFinalizable)?.Finally(context, state);
                }
            }
            finally
            {
                (feature.Policy as IFinalizable)?.Finally(context, FeatureState.Any);
            }
        }

        public IEnumerator<Feature> GetEnumerator() => _toggle.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_toggle).GetEnumerator();
    }
}