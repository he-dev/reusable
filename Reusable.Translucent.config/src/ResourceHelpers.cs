using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [PublicAPI]
    public static class ResourceHelpers
    {
        public static async Task<object?> ReadSettingAsync(this IResource resource, Selector selector, Action<ConfigRequest>? configure = default)
        {
            using var request = ConfigRequest.Create(ResourceMethod.Read, selector, default, configure);
            using var response = await resource.InvokeAsync(request);
            return response.Body;
        }

        public static async Task WriteSettingAsync(this IResource resource, Selector selector, object? newValue, Action<ConfigRequest>? requestAction = default)
        {
            using var request = ConfigRequest.Create(ResourceMethod.Create, selector, newValue, requestAction);
            await resource.InvokeAsync(request);
        }

        #region Getters

        public static async Task<T> ReadSettingAsync<T>(this IResource resource, Selector<T> selector, Action<ConfigRequest>? configure = default)
        {
            return (T)await resource.ReadSettingAsync((Selector)selector, configure)!;
        }

        public static T ReadSetting<T>(this IResource resource, Selector<T> selector, Action<ConfigRequest>? requestAction = default)
        {
            return (T)resource.ReadSettingAsync((Selector)selector, requestAction).GetAwaiter().GetResult()!;
        }

        public static async Task<T> ReadSettingAsync<T>(this IResource resource, Expression<Func<T>> selector, string? index = default, Action<ConfigRequest>? configure = default)
        {
            return (T)await resource.ReadSettingAsync(CreateSelector<T>(selector, index), configure)!;
        }

        public static T ReadSetting<T>(this IResource resource, [NotNull] Expression<Func<T>> selector, string? index = default, Action<ConfigRequest>? configure = default)
        {
            return ReadSettingAsync(resource, selector, index, configure).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        public static async Task WriteSettingAsync<TValue>(this IResource resource, Expression<Func<TValue>> selector, TValue newValue, string? index = default)
        {
            await resource.WriteSettingAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void WriteSetting<T>(this IResource resource, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string? index = default)
        {
            resource.WriteSettingAsync(selector, newValue, index).GetAwaiter().GetResult();
        }

        #endregion

        private static Selector CreateSelector<T>(LambdaExpression selector, string? index)
        {
            return
                index is {}
                    ? new Selector<T>(selector).Index(index)
                    : new Selector<T>(selector);
        }
    }
}