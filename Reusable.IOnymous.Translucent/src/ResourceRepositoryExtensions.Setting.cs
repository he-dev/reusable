using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.IOnymous.Config;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    // Provides CRUD APIs.
    public static partial class ResourceRepositoryExtensions
    {
        public static async Task<object> ReadSettingAsync(this IResourceRepository resourceRepository, Selector selector)
        {
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Get, selector);
            using (var resource = await resourceRepository.InvokeAsync(request))
            {
                var value = await resource.DeserializeAsync();
                var converter = resource.Properties.GetItemOrDefault(SettingProperty.Converter);
                return converter?.Convert(value, resource.Properties.GetDataType()) ?? value;
            }
        }

        public static async Task WriteSettingAsync(this IResourceRepository resourceRepository, Selector selector, object newValue)
        {
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Put, selector, newValue);
            await resourceRepository.InvokeAsync(request);
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

        public static async Task<T> ReadSettingAsync<T>(this IResourceRepository resourceRepository, Selector<T> selector)
        {
            return (T)await resourceRepository.ReadSettingAsync((Selector)selector);
        }

        public static T ReadSetting<T>(this IResourceRepository resourceRepository, Selector<T> selector)
        {
            return (T)resourceRepository.ReadSettingAsync((Selector)selector).GetAwaiter().GetResult();
        }

        public static async Task<T> ReadSettingAsync<T>(this IResourceRepository resourceRepository, Expression<Func<T>> selector, string index = default)
        {
            return (T)await resourceRepository.ReadSettingAsync(CreateSelector<T>(selector, index));
        }

        public static T ReadSetting<T>(this IResourceRepository resourceRepository, [NotNull] Expression<Func<T>> selector, string index = default)
        {
            return ReadSettingAsync(resourceRepository, selector, index).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        public static async Task WriteSettingAsync<TValue>(this IResourceRepository resourceRepository, Expression<Func<TValue>> selector, TValue newValue, string index = default)
        {
            await resourceRepository.WriteSettingAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void WriteSetting<T>(this IResourceRepository resourceRepository, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string index = default)
        {
            resourceRepository.WriteSettingAsync(selector, newValue, index).GetAwaiter().GetResult();
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