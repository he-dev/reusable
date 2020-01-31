using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    public interface IFeatureAgent : IFeatureToggle
    {
        Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default);
    }

    public class FeatureAgent : IFeatureAgent
    {
        private readonly IFeatureToggle _featureToggle;

        public FeatureAgent(IFeatureToggle featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public Feature this[string name] => _featureToggle[name];

        public void AddOrUpdate(Feature feature) => _featureToggle.AddOrUpdate(feature);

        public bool TryRemove(string name, out Feature feature) => _featureToggle.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default)
        {
            ifDisabled ??= () => Task.FromResult<T>(default);

            // Not catching exceptions because the caller should handle them.

            var context = new FeatureContext(this, name, parameter);
            var feature = this[name];

            try
            {
                var state = feature.Policy.State(context);
                try
                {
                    var result = await (state == FeatureState.Enabled ? ifEnabled : ifDisabled)().ConfigureAwait(false);
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

        public IEnumerator<Feature> GetEnumerator() => _featureToggle.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_featureToggle).GetEnumerator();
    }
}