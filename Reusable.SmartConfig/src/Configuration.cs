using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.SmartConfig.Reflection;

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

        private ITypeConverter _converter;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settings = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
            _converter = new JsonSettingConverter();
        }

        /// <summary>
        /// Gets or sets the Func that extracts the member like ignoring the "I" in interface names etc.
        /// </summary>
        [NotNull]
        public Func<Type, string> GetMemberName { get; set; } = SettingMetadata.GetMemberName;

        public static IConfiguration Create(params IResourceProvider[] resourceProviders)
        {
            return new Configuration(new CompositeProvider(resourceProviders));
        }

        public async Task<object> GetItemAsync(LambdaExpression getItem, string handle = null)
        {
            if (getItem == null) throw new ArgumentNullException(nameof(getItem));

            var settingInfo = SettingVisitor.GetSettingInfo(getItem);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            var uri = SettingUriFactory.CreateSettingUri(settingMetadata, handle);
            return await _settings.GetItemAsync<object>(uri, Metadata.Empty.Resource(s => s.Type(settingMetadata.MemberType)));
        }

        public async Task SetItemAsync(LambdaExpression setItem, object newValue, string handle = null)
        {
            if (setItem == null) throw new ArgumentNullException(nameof(setItem));

            var settingInfo = SettingVisitor.GetSettingInfo(setItem);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            var uri = SettingUriFactory.CreateSettingUri(settingMetadata, handle);
            Validate(newValue, settingMetadata.Validations, uri);
            await _settings.SetItemAsync(uri, newValue, Metadata.Empty.Resource(s => s.Type(settingMetadata.MemberType)));            
        }

        private (UriString Uri, Type MemberType) CreateSettingUri(LambdaExpression xItem, string handle)
        {
            var settingInfo = SettingVisitor.GetSettingInfo(xItem);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            return
            (
                SettingUriFactory.CreateSettingUri(settingMetadata, handle),
                settingMetadata.MemberType
            );
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
}