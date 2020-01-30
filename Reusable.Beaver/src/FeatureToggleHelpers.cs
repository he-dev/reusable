using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Beaver.Policies;
using Reusable.Extensions;

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

        public static async Task<FeatureResult<object>> Use(this IFeatureAgent toggle, Feature feature, Func<Task> body, Func<Task>? fallback = default, object? parameter = default)
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

        public static FeatureResult<T> Use<T>(this IFeatureAgent toggle, Feature feature, Func<T> body, Func<T>? fallback = default, object? parameter = default)
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

        public static FeatureResult<object> Use(this IFeatureAgent toggle, Feature feature, Action body, Action? fallback = default, object? parameter = default)
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

        public static FeatureResult<T> Use<T>(this IFeatureAgent toggle, Feature feature, T value, T fallback = default, object? parameter = default)
        {
            return toggle.Use(feature, () => value, () => fallback, parameter);
        }

        #endregion

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