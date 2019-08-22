using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureServiceHelpers
    {
        #region With

        public static IFeatureToggle Update(this IFeatureToggle featureToggle, FeatureIdentifier name, Action<FeatureSelection> update)
        {
            update(new FeatureSelection(featureToggle.Options, name));
            return featureToggle;
        }

        #endregion

        public static bool IsEnabled(this IFeatureToggle toggle, FeatureIdentifier name)
        {
            return toggle.Options[name].Contains(Feature.Options.Enabled);
        }

        public static bool IsLocked(this IFeatureToggle toggle, FeatureIdentifier name)
        {
            return toggle.Options[name].Contains(Feature.Options.Locked);
        }

        // Returns True if options are different from default.
        public static bool IsDirty(this IFeatureToggle toggle, FeatureIdentifier name)
        {
            return toggle.Options.IsDirty(name);
        }

        #region Execute

        public static async Task<T> ExecuteAsync<T>(this IFeatureToggle features, FeatureIdentifier name, Func<Task<T>> body)
        {
            return await features.ExecuteAsync<T>(name, body, () => default(T).ToTask());
        }

        public static async Task ExecuteAsync(this IFeatureToggle features, FeatureIdentifier name, Func<Task> body, Func<Task> fallback)
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

        public static async Task ExecuteAsync(this IFeatureToggle features, FeatureIdentifier name, Func<Task> body)
        {
            await features.ExecuteAsync(name, body, () => Task.FromResult<object>(new object()));
        }

        public static T Execute<T>(this IFeatureToggle featureToggle, FeatureIdentifier name, Func<T> body, Func<T> fallback)
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

        public static void Execute(this IFeatureToggle featureToggle, FeatureIdentifier name, Action body, Action fallback)
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

        public static void Execute(this IFeatureToggle featureToggle, FeatureIdentifier name, Action body)
        {
            featureToggle.Execute(name, body, () => { });
        }

        public static T Switch<T>(this IFeatureToggle featureToggle, FeatureIdentifier name, T value, T fallback)
        {
            return featureToggle.Execute(name, () => value, () => fallback);
        }

        #endregion

        public static IFeatureOptionRepository Update(this IFeatureOptionRepository options, string name, Func<Option<Feature>, Option<Feature>> update)
        {
            options[name] = update(options[name]);
            return options;
        }

        public static IFeatureOptionRepository Batch(this IFeatureOptionRepository options, IEnumerable<string> names, Option<Feature> featureOption, Option<Batch> batchOption)
        {
            foreach (var name in names)
            {
                if (batchOption == Reusable.Beaver.Batch.Options.Set)
                {
                    options[name] = options[name].SetFlag(featureOption);
                }

                if (batchOption == Reusable.Beaver.Batch.Options.Remove)
                {
                    options[name] = options[name].RemoveFlag(featureOption);
                }
            }

            return options;
        }
    }

    public class Batch
    {
        public static class Options
        {
            public static readonly Option<Batch> Set = Option<Batch>.CreateWithCallerName();
            public static readonly Option<Batch> Remove = Option<Batch>.CreateWithCallerName();
        }
    }
}