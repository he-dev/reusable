using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    [UsedImplicitly]
    public static class ResourceProviderExtensions
    {
        public static async Task<object> ReadSettingAsync(this IResourceProvider resources, Selector selector)
        {
            var (uri, metadata) = CreateRequest(selector);
            return await resources.GetItemAsync<object>(uri, metadata);
        }

        public static async Task WriteSettingAsync(this IResourceProvider resources, Selector selector, object newValue)
        {
            var (uri, metadata) = CreateRequest(selector);
            //Validate(newValue, settingMetadata.Validations, uri);
            await resources.SetItemAsync(uri, newValue, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), selector.MemberType));
        }

        #region Helpers

        private static (UriString Uri, IImmutableSession Metadata) CreateRequest(Selector selector)
        {
            var uri = selector.ToString();
            var resources =
                from t in selector.Member.AncestorTypesAndSelf()
                where t.IsDefined(typeof(ResourceAttribute))
                select t.GetCustomAttribute<ResourceAttribute>();
            var resource = resources.FirstOrDefault();

            var metadata =
                ImmutableSession
                    .Empty
                    .SetItem(From<IResourceMeta>.Select(x => x.Type), selector.MemberType)
                    .SetItem(From<IProviderMeta>.Select(x => x.ProviderName), resource?.Provider)
                    .SetItem(From<IResourceMeta>.Select(x => x.ActualName), $"[{selector.Keys.Join(x => x.ToString(), ", ")}]");

            return (uri, metadata);
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