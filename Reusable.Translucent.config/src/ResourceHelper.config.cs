using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidExtensions
    {
        public static async Task<object> ReadSettingAsync(this IResourceSquid resourceSquid, Selector selector)
        {
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Get, selector);
            using (var response = await resourceSquid.InvokeAsync(request))
            {
                if (response.Exists() && response.Body is string body)
                {
                    //var value = await response.DeserializeTextAsync(); //().DeserializeAsync();
                    var converter = response.Metadata.GetItemOrDefault(SettingControllerProperties.Converter);
                    return converter?.Convert(body, selector.DataType);
                }
                else
                {
                    throw DynamicException.Create("SettingNotFount", $"Could not find '{selector}'.");
                }
            }
        }

        public static async Task WriteSettingAsync(this IResourceSquid resourceSquid, Selector selector, object newValue)
        {
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Put, selector, newValue);
            await resourceSquid.InvokeAsync(request);
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

        public static async Task<T> ReadSettingAsync<T>(this IResourceSquid resourceSquid, Selector<T> selector)
        {
            return (T)await resourceSquid.ReadSettingAsync((Selector)selector);
        }

        public static T ReadSetting<T>(this IResourceSquid resourceSquid, Selector<T> selector)
        {
            return (T)resourceSquid.ReadSettingAsync((Selector)selector).GetAwaiter().GetResult();
        }

        public static async Task<T> ReadSettingAsync<T>(this IResourceSquid resourceSquid, Expression<Func<T>> selector, string index = default)
        {
            return (T)await resourceSquid.ReadSettingAsync(CreateSelector<T>(selector, index));
        }

        public static T ReadSetting<T>(this IResourceSquid resourceSquid, [NotNull] Expression<Func<T>> selector, string index = default)
        {
            return ReadSettingAsync(resourceSquid, selector, index).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        public static async Task WriteSettingAsync<TValue>(this IResourceSquid resourceSquid, Expression<Func<TValue>> selector, TValue newValue, string index = default)
        {
            await resourceSquid.WriteSettingAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void WriteSetting<T>(this IResourceSquid resourceSquid, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string index = default)
        {
            resourceSquid.WriteSettingAsync(selector, newValue, index).GetAwaiter().GetResult();
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