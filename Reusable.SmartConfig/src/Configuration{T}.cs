using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    public interface IConfiguration<T>
    {
        Task<TValue> GetItemAsync<TValue>(Expression<Func<T, TValue>> getItem, string handle = default);

        Task SetItemAsync<TValue>(Expression<Func<T, TValue>> setItem, TValue newValue, string handle = default);
    }

    public class Configuration<T> : IConfiguration<T>
    {
        //private static readonly IImmutableSet<MimeType> SupportedTypes = ImmutableHashSet<MimeType>.Empty.Add(MimeType.Text).Add(MimeType.Json);

        //public delegate IConfiguration<T> Factory();

        private readonly IResourceProvider _settingProvider;

        private ITypeConverter _converter;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settingProvider = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
            _converter = new JsonSettingConverter { TrimDoubleQuotes = true };
        }

        [NotNull]
        public ITypeConverter Converter
        {
            get => _converter;
            set => _converter = value ?? throw new ArgumentNullException(nameof(Converter));
        }

        public static IConfiguration Create(params IResourceProvider[] resourceProviders)
        {
            return new Configuration(new CompositeProvider(resourceProviders));
        }

        public async Task<TValue> GetItemAsync<TValue>(Expression<Func<T, TValue>> getItem, string handle = default)
        {
            var settingMetadata = SettingMetadata.FromExpression(getItem, false);
            var uri = SettingUriFactory.CreateSettingUri(settingMetadata, handle);
            var setting = await _settingProvider.GetAsync(uri);

            if (setting.Exists)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await setting.CopyToAsync(memoryStream);

                    if (setting.Format == MimeType.Binary)
                    {
                        var data = await ResourceHelper.DeserializeBinaryAsync<IList<string>>(memoryStream.Rewind());
                        return (TValue)_converter.Convert(data, settingMetadata.MemberType);
                    }

                    using (var streamReader = new StreamReader(memoryStream.Rewind()))
                    {
                        var data = await streamReader.ReadToEndAsync();
                        return (TValue)_converter.Convert(data, settingMetadata.MemberType);
                    }
                }
            }
            else
            {
                throw DynamicException.Create("SettingNotFound", $"Could not find '{uri}'.");
            }
        }

        public async Task SetItemAsync<TValue>(Expression<Func<T, TValue>> setItem, TValue newValue, string handle = default)
        {
            var settingMetadata = SettingMetadata.FromExpression(setItem, false);
            var uri = SettingUriFactory.CreateSettingUri(settingMetadata, handle);

            Validate(newValue, settingMetadata.Validations, uri);
            var data = _converter.Convert<string>(newValue);
            using (var stream = await ResourceHelper.SerializeTextAsync(data))
            {
                await _settingProvider.PutAsync(uri, stream);
            }
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourcePrefixAttribute : Attribute
    {
        public ResourcePrefixAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceSchemeAttribute : Attribute
    {
        public ResourceSchemeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceNameAttribute : Attribute
    {
        public ResourceNameAttribute() { }

        public ResourceNameAttribute(string name)
        {
            Name = name;
        }

        [CanBeNull]
        public string Name { get; }

        public ResourceNameLevel Level { get; set; } = ResourceNameLevel.TypeMember;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class ResourceProviderAttribute : Attribute
    {
        public ResourceProviderAttribute(string name)
        {
            Name = name;
        }
        
        public ResourceProviderAttribute(Type type)
        {
            Type = type;
        }

        public string Name { get;}

        public Type Type { get; }
    }

    public static class SettingQueryStringKeys
    {
        public const string Prefix = nameof(Prefix);
        public const string Handle = nameof(Handle);
        public const string Level = nameof(Level);
        public const string IsCollection = nameof(IsCollection);
    }

    public enum ResourceNameLevel
    {
        NamespaceTypeMember,
        TypeMember,
        Member,
    }
}