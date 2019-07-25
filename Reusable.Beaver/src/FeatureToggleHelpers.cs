using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureServiceHelpers
    {
        public static bool IsEnabled(this IFeatureToggle toggle, string name)
        {
            return toggle.Options[name].Contains(FeatureOption.Enable);
        }

        #region Execute

        public static async Task ExecuteAsync(this IFeatureToggle features, string name, Func<Task> body, Func<Task> fallback)
        {
            await features.ExecuteAsync<object>
            (
                name,
                () =>
                {
                    body();
                    return default;
                },
                () =>
                {
                    fallback();
                    return default;
                }
            );
        }

        public static async Task ExecuteAsync(this IFeatureToggle features, string name, Func<Task> body)
        {
            await features.ExecuteAsync(name, body, () => Task.FromResult<object>(default));
        }

        public static T Execute<T>(this IFeatureToggle featureToggle, string name, Func<T> body, Func<T> fallback)
        {
            return
                featureToggle
                    .ExecuteAsync
                    (
                        name,
                        () => body().ToTask(),
                        () => fallback().ToTask()
                    )
                    .GetAwaiter()
                    .GetResult();
        }

        public static void Execute(this IFeatureToggle featureToggle, string name, Action body, Action fallback)
        {
            featureToggle
                .ExecuteAsync<object>
                (
                    name,
                    () =>
                    {
                        body();
                        return default;
                    },
                    () =>
                    {
                        fallback();
                        return default;
                    }
                )
                .GetAwaiter()
                .GetResult();
        }

        public static void Execute(this IFeatureToggle featureToggle, string name, Action body)
        {
            featureToggle.Execute(name, body, () => { });
        }

        public static T Switch<T>(this IFeatureToggle featureToggle, string name, T value, T fallback)
        {
            return featureToggle.Execute(name, () => value, () => fallback);
        }

        #endregion

        public static IFeatureOptionRepository Batch(this IFeatureOptionRepository options, IEnumerable<string> names, FeatureOption featureOption, BatchOption batchOption)
        {
            foreach (var name in names)
            {
                if (batchOption == BatchOption.Set)
                {
                    options[name] = options[name].SetFlag(featureOption);
                }

                if (batchOption == BatchOption.Remove)
                {
                    options[name] = options[name].RemoveFlag(featureOption);
                }
            }

            return options;
        }
    }

    public class BatchOption : Option<BatchOption>
    {
        public BatchOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly BatchOption Set = CreateWithCallerName();

        public static readonly BatchOption Remove = CreateWithCallerName();
    }
}