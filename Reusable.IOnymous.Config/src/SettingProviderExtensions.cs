using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IOnymous.Config.Annotations;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    [PublicAPI]
    [UsedImplicitly]
    public static class SettingProviderExtensions
    {
        public static async Task<object> ReadSettingAsync(this IResourceProvider resourceProvider, Selector selector)
        {
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Get, selector);
            using (var resource = await resourceProvider.InvokeAsync(request))
            {
                var value = await resource.DeserializeAsync();
                var converter = resource.Properties.GetItemOrDefault(SettingProperty.Converter);
                return converter?.Convert(value, resource.Properties.GetDataType()) ?? value;
            }
        }

        public static async Task WriteSettingAsync(this IResourceProvider resources, Selector selector, object newValue)
        {
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Put, selector, newValue);
            await resources.InvokeAsync(request);
        }

        #region Helpers

        private static object Validate(object value, IEnumerable<ValidationAttribute> validations, UriString uri)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, uri);
            }

            return value;
        }

        #endregion

        #region Getters

        public static async Task<T> ReadSettingAsync<T>(this IResourceProvider resourceProvider, Selector<T> selector)
        {
            return (T)await resourceProvider.ReadSettingAsync((Selector)selector);
        }

        public static T ReadSetting<T>(this IResourceProvider resources, Selector<T> selector)
        {
            return (T)resources.ReadSettingAsync((Selector)selector).GetAwaiter().GetResult();
        }

        public static async Task<T> ReadSettingAsync<T>(this IResourceProvider resourceProvider, Expression<Func<T>> selector, string index = default)
        {
            return (T)await resourceProvider.ReadSettingAsync(CreateSelector<T>(selector, index));
        }

        public static T ReadSetting<T>(this IResourceProvider resources, [NotNull] Expression<Func<T>> selector, string index = default)
        {
            return resources.ReadSettingAsync(selector, index).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        public static async Task WriteSettingAsync<TValue>(this IResourceProvider resources, Expression<Func<TValue>> selector, TValue newValue, string index = default)
        {
            await resources.WriteSettingAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void WriteSetting<T>(this IResourceProvider resources, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string index = default)
        {
            resources.WriteSettingAsync(selector, newValue, index).GetAwaiter().GetResult();
        }

        #endregion

        private static Selector CreateSelector<T>(LambdaExpression selector, string index)
        {
            return
                index is null
                    ? new Selector<T>(selector)
                    : new Selector<T>(selector).Index(index);
        }
    }
}