using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [PublicAPI]
    public static class ResourceProviderExtensions
    {
        public static async Task<object?> ReadSettingAsync(this IResourceRepository resourceRepository, Selector selector, TimeSpan maxAge = default)
        {
            using var request = ConfigRequestBuilder.CreateRequest(RequestMethod.Get, selector, default, r => r.MaxAge = maxAge);
            using var response = await resourceRepository.InvokeAsync(request);
            return response.Body;
        }

        public static async Task WriteSettingAsync(this IResourceRepository resourceRepository, Selector selector, object? newValue)
        {
            using var request = ConfigRequestBuilder.CreateRequest(RequestMethod.Put, selector, newValue);
            await resourceRepository.InvokeAsync(request);
        }

        #region Getters

        public static async Task<T> ReadSettingAsync<T>(this IResourceRepository resourceRepository, Selector<T> selector, TimeSpan maxAge = default)
        {
            return (T)await resourceRepository.ReadSettingAsync((Selector)selector, maxAge)!;
        }

        public static T ReadSetting<T>(this IResourceRepository resourceRepository, Selector<T> selector, TimeSpan maxAge = default)
        {
            return (T)resourceRepository.ReadSettingAsync((Selector)selector, maxAge).GetAwaiter().GetResult()!;
        }

        public static async Task<T> ReadSettingAsync<T>(this IResourceRepository resourceRepository, Expression<Func<T>> selector, string? index = default, TimeSpan maxAge = default)
        {
            return (T)await resourceRepository.ReadSettingAsync(CreateSelector<T>(selector, index), maxAge)!;
        }

        public static T ReadSetting<T>(this IResourceRepository resourceRepository, [NotNull] Expression<Func<T>> selector, string? index = default, TimeSpan maxAge = default)
        {
            return ReadSettingAsync(resourceRepository, selector, index, maxAge).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        public static async Task WriteSettingAsync<TValue>(this IResourceRepository resourceRepository, Expression<Func<TValue>> selector, TValue newValue, string? index = default)
        {
            await resourceRepository.WriteSettingAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void WriteSetting<T>(this IResourceRepository resourceRepository, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string? index = default)
        {
            resourceRepository.WriteSettingAsync(selector, newValue, index).GetAwaiter().GetResult();
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