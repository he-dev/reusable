using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters.Generic;
using Reusable.SmartConfig.Reflection;
using ColorConverter = Reusable.Utilities.JsonNet.Converters.ColorConverter;

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
        private readonly IResourceProvider _settingProvider;

        private ITypeConverter _converter;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settingProvider = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
            _converter = new JsonSettingConverter();
        }

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

            var setting = await _settingProvider.GetAsync(uri, ResourceMetadata.Empty.Type(settingMetadata.MemberType));

            if (setting.Exists)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await setting.CopyToAsync(memoryStream);

                    if (setting.Format == MimeType.Text)
                    {
                        using (var streamReader = new StreamReader(memoryStream.Rewind()))
                        {
                            return await streamReader.ReadToEndAsync();
                        }
                    }

                    if (setting.Format == MimeType.Binary)
                    {
                        return await ResourceHelper.DeserializeBinaryAsync<object>(memoryStream.Rewind());
                    }

                    throw DynamicException.Create
                    (
                        "SettingFormat",
                        $"Setting's '{uri}' format is '{setting.Format}' but only '{MimeType.Binary}' and '{MimeType.Text}' are supported."
                    );
                }
            }
            else
            {
                throw DynamicException.Create("SettingNotFound", $"Could not find '{uri}'.");
            }
        }

        public async Task SetItemAsync(LambdaExpression setItem, object newValue, string handle = null)
        {
            if (setItem == null) throw new ArgumentNullException(nameof(setItem));

            var settingInfo = SettingVisitor.GetSettingInfo(setItem);
            var settingMetadata = new SettingMetadata(settingInfo, GetMemberName);
            var uri = SettingUriFactory.CreateSettingUri(settingMetadata, handle);

            //var settingInfo =
            //    await
            //        resourceProvider
            //            .GetAsync(uri, PopulateProviderInfo(settingMetadata));

            //if (settingInfo.Exists)
            {
                //settingMetadata
                //    .Validations
                //    .Validate(settingName, newValue);

                Validate(newValue, settingMetadata.Validations, uri);

                var resourceMetadata = ResourceMetadata.Empty.Type(settingMetadata.MemberType);

                if (settingMetadata.MemberType == typeof(string))
                {
                    using (var stream = await ResourceHelper.SerializeTextAsync((string)newValue))
                    {
                        await _settingProvider.PutAsync(uri, stream, resourceMetadata.Format(MimeType.Text));
                    }
                }
                else
                {
                    using (var stream = await ResourceHelper.SerializeBinaryAsync(newValue))
                    {
                        await _settingProvider.PutAsync(uri, stream, resourceMetadata.Format(MimeType.Binary));
                    }
                }
            }
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

    [PublicAPI]
    public class JsonSettingConverter : ITypeConverter
    {
        private readonly JsonSerializerSettings _settings;

        private readonly IImmutableSet<Type> _stringTypes;

        private ITypeConverter _internalConverter;

        public JsonSettingConverter(JsonSerializerSettings settings, IEnumerable<Type> stringTypes)
        {
            _settings = settings;
            _stringTypes = stringTypes.ToImmutableHashSet();
            _internalConverter = TypeConverter.Empty;
        }

        public JsonSettingConverter()
            : this
            (
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters =
                    {
                        new StringEnumConverter(),
                        new ColorConverter()
                    }
                }, new[]
                {
                    typeof(string),
                    typeof(Enum),
                    typeof(TimeSpan),
                    typeof(DateTime),
                    typeof(Color)
                }
            ) { }

        public Type FromType => throw new NotSupportedException();

        public Type ToType => throw new NotSupportedException();

        public bool TrimDoubleQuotes { get; set; }

        public bool CanConvert(Type fromType, Type toType)
        {
            return _internalConverter.CanConvert(fromType, toType);
        }

        public object Convert(IConversionContext<object> context)
        {
            if (context.FromType == typeof(string) && context.ToType == typeof(string))
            {
                return TrimDoubleQuotes ? ((string)context.Value).Trim('"') : context.Value;
            }

            if (CanConvert(context.FromType, context.ToType))
            {
                return _internalConverter.Convert(context);
            }
            else
            {
                var newConverterType = GetJsonConverterType(context);
                var newConverter = (ITypeConverter)Activator.CreateInstance(newConverterType, _settings, _stringTypes);
                _internalConverter = _internalConverter.Add(newConverter);
                return Convert(context);
            }
        }

        public bool Equals(ITypeConverter other) => throw new NotSupportedException();

        private Type GetJsonConverterType(IConversionContext<object> context)
        {
            if (context.FromType == typeof(string))
            {
                return typeof(JsonToObjectConverter<>).MakeGenericType(context.ToType);
            }

            if (context.ToType == typeof(string))
            {
                return typeof(ObjectToJsonConverter<>).MakeGenericType(context.FromType);
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}