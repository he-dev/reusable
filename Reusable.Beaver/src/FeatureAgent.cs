using System;
using System.Threading.Tasks;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    public interface IFeatureAgent
    {
        IFeatureToggle FeatureToggle { get; }

        Task<FeatureResult<T>> Use<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default);
    }

    public class FeatureAgent : IFeatureAgent
    {
        public FeatureAgent(IFeatureToggle featureToggle)
        {
            FeatureToggle = featureToggle;
        }

        public IFeatureToggle FeatureToggle { get; }

        public async Task<FeatureResult<T>> Use<T>(Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default)
        {
            ifDisabled ??= () => Task.FromResult<T>(default);

            // Not catching exceptions because the caller should handle them.

            var context = new FeatureContext(FeatureToggle, feature, parameter);
            var policy = FeatureToggle[feature];

            try
            {
                var state = policy.State(context);
                try
                {
                    var result = await (state == FeatureState.Enabled ? ifEnabled : ifDisabled)().ConfigureAwait(false);
                    return new FeatureResult<T>
                    {
                        Value = result,
                        Feature = feature,
                        Policy = policy,
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
                    (policy as IFinalizable)?.Finally(context, state);
                }
            }
            finally
            {
                (policy as IFinalizable)?.Finally(context, FeatureState.Any);
            }
        }
    }
}