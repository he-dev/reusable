using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Foggle
{
    [PublicAPI]
    public static class FeatureServiceHelpers
    {
        #region Execute

        public static async Task ExecuteAsync(this IFeatureService features, string name, Func<Task> body, Func<Task> bodyWhenDisabled)
        {
            await features.ExecuteAsync
            (
                name,
                () => ExecuteAsync(body),
                () => ExecuteAsync(bodyWhenDisabled)
            );

            async Task<object> ExecuteAsync(Func<Task> b)
            {
                await b();
                return default;
            }
        }

        public static async Task ExecuteAsync(this IFeatureService features, string name, Func<Task> body)
        {
            await features.ExecuteAsync(name, body, () => Task.FromResult<object>(default));
        }

        public static void Execute(this IFeatureService features, string name, Action body, Action bodyWhenDisabled)
        {
            features.ExecuteAsync
            (
                name,
                () => ExecuteAsync(body),
                () => ExecuteAsync(bodyWhenDisabled)
            ).GetAwaiter().GetResult();

            Task<object> ExecuteAsync(Action b)
            {
                b();
                return Task.FromResult(default(object));
            }
        }

        public static void Execute(this IFeatureService features, string name, Action body)
        {
            features.Execute(name, body, () => { });
        }

        #endregion

        [NotNull]
        public static IFeatureService Configure(this IFeatureService features, IEnumerable<string> names, Func<FeatureOption, FeatureOption> configure)
        {
            foreach (var name in names)
            {
                features.Configure(name, configure);
            }

            return features;
        }
    }
}