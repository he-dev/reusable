using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureServiceHelpers
    {
        public static bool IsEnabled(this IFeatureToggle toggle, Feature name, object? parameter = default)
        {
            return toggle[name].IsEnabled(new Feature(name)
            {
                Toggle = toggle,
                Parameter = parameter
            });
        }

        public static bool IsLocked(this IFeatureToggle toggle, Feature name)
        {
            return (toggle[name] as Lock) is {};
        }

        #region Execute helpers

        public static async Task IIf(this IFeatureToggle toggle, Feature name, Func<Task> body, Func<Task>? fallback = default)
        {
            await toggle.IIf<object>
            (
                name,
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

        public static T IIf<T>(this IFeatureToggle toggle, Feature name, Func<T> body, Func<T>? fallback = default)
        {
            return
                toggle
                    .IIf
                    (
                        name,
                        () => body().ToTask(),
                        () => (fallback ?? (() => default))().ToTask()
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static void IIf(this IFeatureToggle toggle, Feature name, Action body, Action? fallback = default)
        {
            toggle
                .IIf
                (
                    name,
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

        public static T IIf<T>(this IFeatureToggle toggle, Feature name, T value, T fallback = default)
        {
            return toggle.IIf(name, () => value, () => fallback);
        }

        #endregion
    }
}