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
        public static FeatureState State(this IFeatureToggle toggle, string name, object? parameter = default) => toggle[name].Policy.State(new FeatureContext(toggle, name, parameter));

        public static bool IsEnabled(this IFeatureToggle toggle, string name, object? parameter = default) => toggle[name].Policy.IsEnabled(new FeatureContext(toggle, name, parameter));

        public static bool IsLocked(this IFeatureToggle toggle, string name) => toggle[name].Policy is Lock;

        public static IFeatureToggle Enable(this IFeatureToggle toggle, string name) => toggle.SetPolicy(name, FeaturePolicy.AlwaysOn);

        public static IFeatureToggle Disable(this IFeatureToggle toggle, string name) => toggle.SetPolicy(name, FeaturePolicy.AlwaysOff);

        public static IFeatureToggle Lock(this IFeatureToggle toggle, string name) => toggle.SetPolicy(name, toggle[name].Policy.Lock());
        
        public static IFeatureToggle Telemetry(this IFeatureToggle toggle, string name, bool telemetry) => toggle.SetPolicy($"{name}.{nameof(Telemetry)}", telemetry ? FeaturePolicy.AlwaysOn : FeaturePolicy.AlwaysOff);

        #region Use

        public static async Task<FeatureResult<object>> Use(this IFeatureAgent toggle, string name, Func<Task> body, Func<Task>? fallback = default, object? parameter = default)
        {
            return await toggle.Use<object>
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
                },
                parameter
            );
        }

        public static FeatureResult<T> Use<T>(this IFeatureAgent toggle, string name, Func<T> body, Func<T>? fallback = default, object? parameter = default)
        {
            return
                toggle
                    .Use
                    (
                        name,
                        () => body().ToTask(),
                        () => (fallback ?? (() => default))().ToTask(),
                        parameter
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureResult<object> Use(this IFeatureAgent toggle, string name, Action body, Action? fallback = default, object? parameter = default)
        {
            return
                toggle
                    .Use
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
                        },
                        parameter
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureResult<T> Use<T>(this IFeatureAgent toggle, string name, T value, T fallback = default, object? parameter = default)
        {
            return toggle.Use(name, () => value, () => fallback, parameter);
        }

        #endregion

        // --------

        public static IFeatureToggle SetPolicy(this IFeatureToggle toggle, string name, IFeaturePolicy policy, bool telemetry = true)
        {
            var feature = toggle[name];

            if (feature is Feature.Fallback)
            {
                toggle[name] = new Feature(name) { Policy = policy };
            }
            else
            {
                feature.Policy = policy;
            }

            return toggle;
        }

        public static IFeatureToggle Remove(this IFeatureToggle toggle, string name)
        {
            toggle[name] = new Feature.Remove();
            return toggle;
        }
    }
}