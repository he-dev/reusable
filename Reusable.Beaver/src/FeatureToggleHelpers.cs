using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureServiceHelpers
    {
        public static bool IsEnabled(this IFeatureToggle toggle, Feature feature) => toggle[feature].IsEnabled(new Feature(feature) { Toggle = toggle });

        public static bool IsLocked(this IFeatureToggle toggle, Feature feature) => (toggle[feature] as Lock) is {};

        public static IFeatureToggle Enable(this IFeatureToggle toggle, Feature feature) => toggle.AddOrUpdate(new AlwaysOn(feature));

        public static IFeatureToggle Disable(this IFeatureToggle toggle, Feature feature) => toggle.AddOrUpdate(new AlwaysOff(feature));

        public static IFeatureToggle Lock(this IFeatureToggle toggle, Feature feature) => toggle.AddOrUpdate(toggle[feature].Lock());

        #region IIf helpers

        public static async Task<FeatureActionResult<object>> IIf(this IFeatureToggle toggle, Feature feature, Func<Task> body, Func<Task>? fallback = default)
        {
            return await toggle.IIf<object>
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
                }
            );
        }

        public static FeatureActionResult<T> IIf<T>(this IFeatureToggle toggle, Feature feature, Func<T> body, Func<T>? fallback = default)
        {
            return
                toggle
                    .IIf
                    (
                        feature,
                        () => body().ToTask(),
                        () => (fallback ?? (() => default))().ToTask()
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureActionResult<object> IIf(this IFeatureToggle toggle, Feature feature, Action body, Action? fallback = default)
        {
            return
                toggle
                    .IIf
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
                        }
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureActionResult<T> IIf<T>(this IFeatureToggle toggle, Feature feature, T value, T fallback = default)
        {
            return toggle.IIf(feature, () => value, () => fallback);
        }

        #endregion
    }
}