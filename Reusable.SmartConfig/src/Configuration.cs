using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.IOnymous;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.SmartConfig
{
    public interface IConfiguration
    {
        Task<object> GetItemAsync(LambdaExpression getItem, string handle = default);

        Task SetItemAsync(LambdaExpression setItem, object newValue, string handle = default);
    }

    [PublicAPI]
    [UsedImplicitly]
    public class Configuration : IConfiguration
    {
        private readonly IResourceProvider _settings;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settings = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
        }

        /// <summary>
        /// Gets or sets the Func that extracts the member like ignoring the "I" in interface names etc.
        /// </summary>
        [NotNull]
        public Func<Type, string> GetMemberName { get; set; } = SettingMetadata.GetMemberName;

        public async Task<object> GetItemAsync(LambdaExpression getItem, string handle = null)
        {
            if (getItem == null) throw new ArgumentNullException(nameof(getItem));

            var settingInfo = MemberVisitor.GetMemberInfo(getItem);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            var (uri, metadata) = SettingRequestFactory.CreateSettingRequest(settingMetadata, handle);
            return await _settings.GetItemAsync<object>(uri, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), settingMetadata.MemberType));
        }

        public async Task<object> GetItemAsync(Selector selector)
        {
            var uri = selector.ToString();

            var settingInfo = MemberVisitor.GetMemberInfo(selector.Expression);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            var (_, metadata) = SettingRequestFactory.CreateSettingRequest(settingMetadata);
            metadata =
                metadata
                    .SetItem(From<IResourceMeta>.Select(x => x.Type), settingMetadata.MemberType)
                    .SetItem(From<IProviderMeta>.Select(x => x.CustomName), settingMetadata.ResourceProviderName)
                    .SetItem(From<IProviderMeta>.Select(x => x.DefaultName), settingMetadata.ResourceProviderType?.ToPrettyString());
            return await _settings.GetItemAsync<object>(uri, metadata);
        }

        public async Task SetItemAsync(LambdaExpression setItem, object newValue, string handle = null)
        {
            if (setItem == null) throw new ArgumentNullException(nameof(setItem));

            var settingInfo = MemberVisitor.GetMemberInfo(setItem);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            var (uri, metadata) = SettingRequestFactory.CreateSettingRequest(settingMetadata, handle);
            Validate(newValue, settingMetadata.Validations, uri);
            await _settings.SetItemAsync(uri, newValue, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), settingMetadata.MemberType));
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