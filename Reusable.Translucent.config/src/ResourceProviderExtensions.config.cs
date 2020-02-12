using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Quickey;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    [PublicAPI]
    public static class ResourceProviderExtensions
    {
        public static async Task<object?> ReadSettingAsync(this IResource resource, Selector selector, TimeSpan maxAge = default)
        {
            using var request = ConfigRequestBuilder.CreateRequest(ResourceMethod.Get, selector, default, r => r.MaxAge = maxAge);
            using var response = await resource.InvokeAsync(request);
            return response.Body;
        }

        public static async Task WriteSettingAsync(this IResource resource, Selector selector, object? newValue)
        {
            using var request = ConfigRequestBuilder.CreateRequest(ResourceMethod.Put, selector, newValue);
            await resource.InvokeAsync(request);
        }

        #region Getters

        public static async Task<T> ReadSettingAsync<T>(this IResource resource, Selector<T> selector, TimeSpan maxAge = default)
        {
            return (T)await resource.ReadSettingAsync((Selector)selector, maxAge)!;
        }

        public static T ReadSetting<T>(this IResource resource, Selector<T> selector, TimeSpan maxAge = default)
        {
            return (T)resource.ReadSettingAsync((Selector)selector, maxAge).GetAwaiter().GetResult()!;
        }

        public static async Task<T> ReadSettingAsync<T>(this IResource resource, Expression<Func<T>> selector, string? index = default, TimeSpan maxAge = default)
        {
            return (T)await resource.ReadSettingAsync(CreateSelector<T>(selector, index), maxAge)!;
        }

        public static T ReadSetting<T>(this IResource resource, [NotNull] Expression<Func<T>> selector, string? index = default, TimeSpan maxAge = default)
        {
            return ReadSettingAsync(resource, selector, index, maxAge).GetAwaiter().GetResult();
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