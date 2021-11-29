using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable.FeatureBuzz
{
    public static class FeatureCollectionExtensions
    {
        public static FeatureState State(this IFeatureCollection features, string name, object? parameter = default)
        {
            return features[name].Let(f => f.Policy.State(features, name, parameter));
        }

        public static bool IsEnabled(this IFeatureCollection features, string name, object? parameter = default)
        {
            return features[name].Let(f => f.Policy.IsEnabled(features, name, parameter));
        }

        public static bool IsLocked(this IFeatureCollection features, string name) => features[name].AllowPolicyChange == false;

        public static IFeatureCollection Enable(this IFeatureCollection features, string name) => features.Also(t => t[name].Policy = FeaturePolicy.Enabled);

        public static IFeatureCollection Disable(this IFeatureCollection features, string name) => features.Also(t => t[name].Policy = FeaturePolicy.Disabled);

        public static IFeatureCollection Lock(this IFeatureCollection features, string name) => features.Also(t => t[name].AllowPolicyChange = false);

        public static IFeatureCollection Telemetry(this IFeatureCollection features, string name, IFeaturePolicy policy)
        {
            return features.Also(f => f.TryAdd(new Feature.Telemetry(name, policy)));
        }

        public static bool TryAdd(this IFeatureCollection features, string name, IFeaturePolicy policy, params string[] tags)
        {
            return features.TryAdd(new Feature(name, policy, tags.ToImmutableHashSet(SoftString.Comparer)));
        }

        public static void ForEach(this IEnumerable<Feature> features, IImmutableSet<string> tags, Action<Feature> configure)
        {
            foreach (var feature in features.Where(f => tags.IsSubsetOf(f.Tags)))
            {
                configure(feature);
            }
        }
        
        public static async Task<FeatureResult<object>> Use(this IFeatureCollection features, string name, Func<Task> onEnabled, Func<Task>? onDisabled = default, object? parameter = default)
        {
            return await features.Use<object>
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

        public static FeatureResult<T> Use<T>(this IFeatureCollection features, string name, Func<T> onEnabled, Func<T>? onDisabled = default, object? parameter = default)
        {
            return
                features
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

        public static FeatureResult<object?> Use(this IFeatureCollection features, string name, Action onEnabled, Action? onDisabled = default, object? parameter = default)
        {
            return
                features
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

        public static FeatureResult<T> Use<T>(this IFeatureCollection features, string name, T onEnabled, T onDisabled = default, object? parameter = default)
        {
            return features.Use(name, () => onEnabled, () => onDisabled, parameter);
        }

        #region IEnumerable

        public static void Add(this IFeatureCollection features, string name, IFeaturePolicy policy)
        {
            features.TryAdd(name, policy);
        }

        #endregion
    }
}