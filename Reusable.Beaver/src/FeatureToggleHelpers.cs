using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Beaver.Policies;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureToggleHelpers
    {
        public static FeatureState State(this IFeatureToggle toggle, Feature feature, object? parameter = default) => toggle[feature].State(new FeatureContext(toggle, feature, parameter));

        public static bool IsEnabled(this IFeatureToggle toggle, Feature feature, object? parameter = default) => toggle[feature].IsEnabled(new FeatureContext(toggle, feature, parameter));

        public static bool IsLocked(this IFeatureToggle toggle, Feature feature) => (toggle[feature] as Lock) is {};

        public static IFeatureToggle Enable(this IFeatureToggle toggle, Feature feature) => toggle.SetOrUpdate(feature, FeaturePolicy.AlwaysOn);

        public static IFeatureToggle Disable(this IFeatureToggle toggle, Feature feature) => toggle.SetOrUpdate(feature, FeaturePolicy.AlwaysOff);

        public static IFeatureToggle Lock(this IFeatureToggle toggle, Feature feature) => toggle.SetOrUpdate(feature, toggle[feature].Lock());

        #region Use

        public static async Task<FeatureResult<T>> Use<T>(this IFeatureToggle toggle, Feature feature, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default)
        {
            ifDisabled ??= () => Task.FromResult<T>(default);

            // Not catching exceptions because the caller should handle them.

            var context = new FeatureContext(toggle, feature, parameter);
            var policy = toggle[feature];

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

        public static async Task<FeatureResult<object>> Use(this IFeatureToggle toggle, Feature feature, Func<Task> body, Func<Task>? fallback = default, object? parameter = default)
        {
            return await toggle.Use<object>
            (
                feature,
                async () =>
                {
                    await body();
                    return Task.CompletedTask;
                },
                async () =>
                {
                    await (fallback ?? (() => Task.CompletedTask))();
                    return Task.CompletedTask;
                },
                parameter
            );
        }

        public static FeatureResult<T> Use<T>(this IFeatureToggle toggle, Feature feature, Func<T> body, Func<T>? fallback = default, object? parameter = default)
        {
            return
                toggle
                    .Use
                    (
                        feature,
                        () => body().ToTask(),
                        () => (fallback ?? (() => default))().ToTask(),
                        parameter
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureResult<object> Use(this IFeatureToggle toggle, Feature feature, Action body, Action? fallback = default, object? parameter = default)
        {
            return
                toggle
                    .Use
                    (
                        feature,
                        () =>
                        {
                            body();
                            return default(object?).ToTask();
                        },
                        () =>
                        {
                            (fallback ?? (() => { }))();
                            return default(object?).ToTask();
                        },
                        parameter
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureResult<T> Use<T>(this IFeatureToggle toggle, Feature feature, T value, T fallback = default, object? parameter = default)
        {
            return toggle.Use(feature, () => value, () => fallback, parameter);
        }

        #endregion

        /// <summary>
        /// Logs feature-telemetry when it's tagged with the 'telemetry' tag.
        /// </summary>
        public static async Task<FeatureResult<T>> Telemetry<T>(this IFeatureToggle toggle, ILogger logger, Func<IFeatureToggle, Task<FeatureResult<T>>> use)
        {
            using (logger.BeginScope().WithCorrelationHandle("UseFeature").UseStopwatch())
            {
                return await use(toggle).ContinueWith(t =>
                {
                    if (t.Result.Feature.Tags.Contains(nameof(Telemetry)))
                    {
                        logger.Log(Abstraction.Layer.Service().Meta(new
                        {
                            FeatureTelemetry = new
                            {
                                name = t.Result.Feature.Name,
                                state = t.Result.State,
                                policy = t.Result.Policy.GetType().ToPrettyString()
                            }
                        }), log => log.Exception(t.Exception));
                    }

                    return t.Result;
                });
            }
        }

        /// <summary>
        /// Logs feature-telemetry when it's tagged with the 'telemetry' tag.
        /// </summary>
        public static FeatureResult<T> Telemetry<T>(this IFeatureToggle toggle, ILogger logger, Func<IFeatureToggle, FeatureResult<T>> use)
        {
            return toggle.Telemetry(logger, t => use(t).ToTask()).GetAwaiter().GetResult();
        }

        // --------

        public static IFeatureToggle SetOrUpdate(this IFeatureToggle toggle, Feature feature, IFeaturePolicy policy)
        {
            toggle[feature] = policy;
            return toggle;
        }

        public static IFeatureToggle Remove(this IFeatureToggle toggle, Feature feature)
        {
            toggle[feature] = FeaturePolicy.Remove;
            return toggle;
        }
    }
}