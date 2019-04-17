using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        private static readonly IImmutableSet<MimeType> SupportedTypes = ImmutableHashSet<MimeType>.Empty.Add(MimeType.Text).Add(MimeType.Json);

        private readonly IResourceProvider _settingProvider;

        private ITypeConverter _converter;

        public Configuration([NotNull] IResourceProvider settingProvider)
        {
            _settingProvider = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
            _converter = new JsonSettingConverter();
        }

        [NotNull]
        public ITypeConverter Converter
        {
            get => _converter;
            set => _converter = value ?? throw new ArgumentNullException(nameof(Converter));
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

            var setting = await _settingProvider.GetAsync(uri);

            if (setting.Exists)
            {
                if (!SupportedTypes.Contains(setting.Format))
                {
                    throw DynamicException.Create("UnsupportedSettingFormat", $"'{setting.Format}' is not supported.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await setting.CopyToAsync(memoryStream);

                    //             {
                    //                 var data = await ResourceHelper.DeserializeBinaryAsync<IList<string>>(memoryStream.Rewind());
                    //                 return (TValue)_converter.Convert(data, settingMetadata.MemberType);
                    //             }

                    using (var streamReader = new StreamReader(memoryStream.Rewind()))
                    {
                        var json = streamReader.ReadToEnd();
                        //var converter = GetOrAddDeserializer(settingMetadata.MemberType);
                        return Converter.Convert(json, settingMetadata.MemberType);
                    }
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
                var json = Converter.Convert<string>(newValue);
                using (var stream = await ResourceHelper.SerializeTextAsync(json))
                {
                    await _settingProvider.PutAsync(uri, stream);
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

        //        /// <summary>
        //        /// Assigns the same setting value to the specified member.
        //        /// </summary>
        //        public void BindSetting<T>(Expression<Func<T>> expression, string instanceName = null)
        //        {
        //            if (expression == null) throw new ArgumentNullException(nameof(expression));
        //
        //            var settingMetadata = SettingMetadata.FromExpression(expression, false);
        //            var uri = settingMetadata.CreateUri(instanceName);
        //            var value = GetSetting(expression, instanceName);
        //            settingMetadata.SetValue(Validate(value, settingMetadata.Validations, uri));
        //        }
        //
        //        /// <summary>
        //        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        //        /// </summary>        
        //        public void BindSettings<T>(T obj, string instanceName = null)
        //        {
        //            if (obj == null) throw new ArgumentNullException(nameof(obj));
        //
        //            var settingProperties =
        //                typeof(T)
        //                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        //                    .Where(p => p.IsDefined(typeof(SettingMemberAttribute)));
        //
        //            foreach (var property in settingProperties)
        //            {
        //                // This expression allows to reuse GeAsync.
        //                var expression = Expression.Lambda(
        //                    Expression.Property(
        //                        Expression.Constant(obj),
        //                        property.Name
        //                    )
        //                );
        //
        //                var value = GetSetting(expression, instanceName);
        //                var settingMetadata = SettingMetadata.FromExpression(expression, false);
        //                var uri = settingMetadata.CreateUri(instanceName);
        //                settingMetadata.SetValue(Validate(value, settingMetadata.Validations, uri));
        //            }
        //        }

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

    public interface IConfiguration<T> : IConfiguration { }

    public class Configuration<T> : Configuration, IConfiguration<T>
    {
        public Configuration([NotNull] IResourceProvider settingProvider) : base(settingProvider) { }
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

    public static class ConfigurationExtensions
    {
        public static async Task<TValue> GetItemAsync<TValue>(this IConfiguration configuration, Expression<Func<TValue>> getItem, string handle = default)
        {
            return (TValue)await configuration.GetItemAsync(getItem, handle);
        }

        public static async Task SetItemAsync<TValue>(this IConfiguration configuration, Expression<Func<TValue>> setItem, TValue newValue, string handle = default)
        {
            await configuration.SetItemAsync(setItem, newValue, handle);
        }

        [Obsolete("Use GetItem")]
        public static T GetSetting<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] string handle = null)
        {
            return (T)configuration.GetItemAsync(expression, handle).GetAwaiter().GetResult();
        }

        [Obsolete("Use SetItem")]
        public static void SaveSetting<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string handle = null)
        {
            configuration.SetItemAsync(expression, newValue, handle).GetAwaiter().GetResult();
        }

        public static T GetItem<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] string handle = null)
        {
            return (T)configuration.GetItemAsync(expression, handle).GetAwaiter().GetResult();
        }

        public static void SetItem<T>(this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string handle = null)
        {
            configuration.SetItemAsync(expression, newValue, handle).GetAwaiter().GetResult();
        }

        public static async Task<TValue> GetItemAsync<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> getItem, string handle = default)
        {
            return (TValue)await configuration.GetItemAsync(getItem, handle);
        }

        public static async Task SetItemAsync<T, TValue>(this IConfiguration<T> configuration, Expression<Func<T, TValue>> setItem, TValue newValue, string handle = default)
        {
            await configuration.SetItemAsync(setItem, newValue, handle);
        }

        //        public static T GetSetting<T>(this IConfiguration configuration, string name, string instance = default)
        //        {
        //            var expression =
        //                Expression.Lambda<Func<T>>(
        //                    Expression.Property(
        //                        Expression.Constant(default(T), typeof(T)),
        //                        name
        //                    )
        //                );
        //            return configuration.GetSetting(expression, instance);
        //        }
    }
}