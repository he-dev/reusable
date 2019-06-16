using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureServiceHelpers
    {
        #region Execute

        public static async Task ExecuteAsync(this IFeatureToggle features, string name, Func<Task> bodyWhenOn, Func<Task> bodyWhenOff)
        {
            await features.ExecuteAsync
            (
                name,
                () => ExecuteAsync(bodyWhenOn),
                () => ExecuteAsync(bodyWhenOff)
            );

            async Task<object> ExecuteAsync(Func<Task> b)
            {
                await b();
                return default;
            }
        }

        public static async Task ExecuteAsync(this IFeatureToggle features, string name, Func<Task> bodyWhenOn)
        {
            await features.ExecuteAsync(name, bodyWhenOn, () => Task.FromResult<object>(default));
        }

        public static void Execute(this IFeatureToggle features, string name, Action bodyWhenOn, Action bodyWhenOff)
        {
            features.ExecuteAsync
            (
                name,
                () => ExecuteAsync(bodyWhenOn),
                () => ExecuteAsync(bodyWhenOff)
            ).GetAwaiter().GetResult();

            Task<object> ExecuteAsync(Action b)
            {
                b();
                return Task.FromResult(default(object));
            }
        }

        public static void Execute(this IFeatureToggle features, string name, Action bodyWhenOn)
        {
            features.Execute(name, bodyWhenOn, () => { });
        }

        #endregion

        [NotNull]
        public static IFeatureToggle Configure(this IFeatureToggle features, IEnumerable<string> names, Func<FeatureOption, FeatureOption> configure)
        {
            foreach (var name in names)
            {
                features.Configure(name, configure);
            }

            return features;
        }
    }
}