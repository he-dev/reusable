using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.IOnymous;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Reflection;

namespace Reusable.SmartConfig
{
    public interface IConfiguration
    {
        Task<object> GetItemAsync(Selector selector);

        Task SetItemAsync(Selector selector, object newValue);
    }

    [PublicAPI]
    [UsedImplicitly]
    public class Configuration : IConfiguration
    {
        public static readonly ITypeConverter<UriString, string> DefaultUriStringConverter = new UriStringToStringConverter();

        public static readonly IEnumerable<SoftString> DefaultSchemes = ImmutableList<SoftString>.Empty.Add("config");
        
        private readonly IResourceProvider _settings;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settings = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
        }

        /// <summary>
        /// Gets or sets the Func that extracts the member like ignoring the "I" in interface names etc.
        /// </summary>
        //[NotNull]
        //public Func<Type, string> GetMemberName { get; set; } = SettingMetadata.GetMemberName;

//        public async Task<object> GetItemAsync(LambdaExpression getItem, string handle = null)
//        {
//            if (getItem == null) throw new ArgumentNullException(nameof(getItem));
//
//            var settingInfo = MemberVisitor.GetMemberInfo(getItem);
//            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
//            var (uri, metadata) = SettingRequestFactory.CreateSettingRequest(settingMetadata, handle);
//            return await _settings.GetItemAsync<object>(uri, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), settingMetadata.MemberType));
//        }

        public async Task<object> GetItemAsync(Selector selector)
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
                    .SetItem(From<IResourceMeta>.Select(x => x.ActualName), uri);
            
            return await _settings.GetItemAsync<object>(uri, metadata);
        }

//        public async Task SetItemAsync(LambdaExpression setItem, object newValue, string handle = null)
//        {
//            if (setItem == null) throw new ArgumentNullException(nameof(setItem));
//
//            var settingInfo = MemberVisitor.GetMemberInfo(setItem);
//            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
//            var (uri, metadata) = SettingRequestFactory.CreateSettingRequest(settingMetadata, handle);
//            Validate(newValue, settingMetadata.Validations, uri);
//            await _settings.SetItemAsync(uri, newValue, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), settingMetadata.MemberType));
//        }
        
        public async Task SetItemAsync(Selector selector, object newValue)
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
                    .SetItem(From<IProviderMeta>.Select(x => x.ProviderName), resource?.Provider);
            
            //Validate(newValue, settingMetadata.Validations, uri);
            await _settings.SetItemAsync(uri, newValue, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), selector.MemberType));
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
    }

    public static class ResourceProviderExtensions
    {
        public static async Task<T> GetSetting<T>(this IResourceProvider resources, Selector<T> key)
        {
            //            var settingInfo = MemberVisitor.GetMemberInfo(getItem);
            //            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            //            var (uri, metadata) = SettingRequestFactory.CreateSettingRequest(settingMetadata, handle);
            //            return await _settings.GetItemAsync<object>(uri, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), settingMetadata.MemberType));

            return default;
        }
    }
}