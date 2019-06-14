using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.IOnymous;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;

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

        public static readonly IImmutableList<SoftString> DefaultSchemes = ImmutableList<SoftString>.Empty.Add("config");

        private readonly IResourceProvider _settings;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settings = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
        }
        
        public async Task<object> GetItemAsync(Selector selector)
        {
            var (uri, metadata) = CreateRequest(selector);
            return await _settings.GetItemAsync<object>(uri, metadata);
        }

        public async Task SetItemAsync(Selector selector, object newValue)
        {
            var (uri, metadata) = CreateRequest(selector);
            //Validate(newValue, settingMetadata.Validations, uri);
            await _settings.SetItemAsync(uri, newValue, metadata.SetItem(From<IResourceMeta>.Select(x => x.Type), selector.MemberType));
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
    }
}