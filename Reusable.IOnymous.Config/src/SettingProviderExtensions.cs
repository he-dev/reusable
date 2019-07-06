using System;
using System.Collections.Generic;
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
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    [PublicAPI]
    [UsedImplicitly]
    public static class SettingProviderExtensions
    {
        public static async Task<object> ReadSettingAsync(this IResourceProvider resources, Selector selector)
        {
            var request = await CreateRequestAsync(RequestMethod.Get, selector);
            //return await resources.ReadObjectAsync<object>(request);

            using (var item = await resources.InvokeAsync(request))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await item.CopyToAsync(memoryStream);

                    if (item.Format == MimeType.Text)
                    {
                        return await ResourceHelper.DeserializeTextAsync(memoryStream.Rewind());
                    }

                    if (item.Format == MimeType.Binary)
                    {
                        return await ResourceHelper.DeserializeBinaryAsync<object>(memoryStream.Rewind());
                    }
                }

                throw DynamicException.Create
                (
                    $"ItemFormat",
                    $"Item's '{request.Uri}' format is '{item.Format}' but only '{MimeType.Binary}' and '{MimeType.Text}' are supported."
                );
            }
        }

        public static async Task WriteSettingAsync(this IResourceProvider resources, Selector selector, object newValue)
        {
            var request = await CreateRequestAsync(RequestMethod.Put, selector, newValue);
            //Validate(newValue, settingMetadata.Validations, uri);
            await resources.InvokeAsync(request);
        }

        #region Helpers

        private static async Task<Request> CreateRequestAsync(RequestMethod method, Selector selector, object value = default)
        {
            var uri = selector.ToString();
            var resources =
                from t in selector.Members()
                where t.IsDefined(typeof(ResourceAttribute))
                select t.GetCustomAttribute<ResourceAttribute>();
            var resource = resources.FirstOrDefault();

            var properties =
                ImmutableContainer
                    .Empty
                    .SetItem(Resource.Property.DataType, selector.MemberType)
                    // request.Properties.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Type)) == typeof(string)
                    //.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), resource?.Provider.ToSoftString())
                    .SetItem(Resource.Property.ActualName, $"[{selector.Join(x => x.ToString(), ", ")}]")
                    .SetName(resource?.Provider.ToSoftString());

            var request = new Request
            {
                Uri = uri,
                Method = method,
                Properties = properties,
            };

            AddBody(request, value);

            return request;
        }

        private static void AddBody(Request request, object value)
        {
            switch (value)
            {
                case null:
                    break;
                case string str:
                    request.CreateBodyStreamFunc = () => ResourceHelper.SerializeTextAsync(str);
                    request.SetProperties(p => p.SetItem(Resource.Property.Format, MimeType.Text));
                    break;
                default:
                    request.CreateBodyStreamFunc = () => ResourceHelper.SerializeBinaryAsync(value);
                    request.SetProperties(p => p.SetItem(Resource.Property.Format, MimeType.Binary));
                    break;
            }
        }

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

        public static async Task<T> ReadSettingAsync<T>(this IResourceProvider resources, Selector<T> selector)
        {
            return (T)await resources.ReadSettingAsync((Selector)selector);
        }

        public static T ReadSetting<T>(this IResourceProvider resources, Selector<T> selector)
        {
            return (T)resources.ReadSettingAsync((Selector)selector).GetAwaiter().GetResult();
        }

        public static async Task<T> ReadSettingAsync<T>(this IResourceProvider resources, Expression<Func<T>> selector, string index = default)
        {
            return (T)await resources.ReadSettingAsync(CreateSelector<T>(selector, index));
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