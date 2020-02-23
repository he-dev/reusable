using System;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable.Beaver
{
    public static class FeatureControllerExtensions
    {
        public static async Task<FeatureResult<object>> Use(this IFeatureController toggle, string name, Func<Task> onEnabled, Func<Task>? onDisabled = default, object? parameter = default)
        {
            return await toggle.Use<object>
            (
                name,
                async () =>
                {
                    await onEnabled();
                    return Task.CompletedTask;
                },
                async () =>
                {
                    await (onDisabled ?? (() => Task.CompletedTask))();
                    return Task.CompletedTask;
                },
                parameter
            );
        }

        public static FeatureResult<T> Use<T>(this IFeatureController toggle, string name, Func<T> onEnabled, Func<T>? onDisabled = default, object? parameter = default)
        {
            return
                toggle
                    .Use
                    (
                        name,
                        () => onEnabled().ToTask(),
                        () => (onDisabled ?? (() => default(T)))().ToTask(),
                        parameter
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureResult<object?> Use(this IFeatureController toggle, string name, Action onEnabled, Action? onDisabled = default, object? parameter = default)
        {
            return
                toggle
                    .Use
                    (
                        name,
                        () =>
                        {
                            onEnabled();
                            return default(object?).ToTask();
                        },
                        () =>
                        {
                            (onDisabled ?? (() => { }))();
                            return default(object?).ToTask();
                        },
                        parameter
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static FeatureResult<T> Use<T>(this IFeatureController toggle, string name, T onEnabled, T onDisabled = default, object? parameter = default)
        {
            return toggle.Use(name, () => onEnabled, () => onDisabled, parameter);
        }
    }
}