using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters.Generic;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    //[UsedImplicitly]
    public interface IConfiguration
    {
        T GetSetting<T>([NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null);
        
        void SaveSetting<T>([NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null);
        
        void BindSetting<T>([NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null);

        void BindSettings<T>([NotNull] T obj, [CanBeNull] string instanceName = null);
    }

    public class Configuration : IConfiguration
    {
        private readonly IResourceProvider _settingProvider;

        public Configuration(IEnumerable<IResourceProvider> settingProviders)
        {
            _settingProvider = new CompositeResourceProvider(settingProviders.Where(x => x.Schemes.Contains("setting")));
        }

        public T GetSetting<T>([NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)GetSetting((LambdaExpression)expression, instanceName);
        }

        private object GetSetting([NotNull] LambdaExpression expression, [CanBeNull] string instanceName = null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);
            var settingInfo =
                _settingProvider
                    .GetAsync(uri, PopulateProviderInfo(settingMetadata)).GetAwaiter().GetResult();

            if (settingInfo.Exists)
            {
                using (var memoryStream = new MemoryStream())
                using (var streamReader = new StreamReader(memoryStream))
                {
                    settingInfo.CopyToAsync(memoryStream).GetAwaiter().GetResult();
                    memoryStream.TryRewind();
                    var json = streamReader.ReadToEnd();
                    var converter = GetOrAddDeserializer(settingMetadata.MemberType);
                    return converter.Convert(json, settingMetadata.MemberType);
                }
            }
            else
            {
                throw DynamicException.Create("SettingNotFound", $"Could not find '{uri}'.");
            }
        }

        public void SaveSetting<T>([NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);

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
                var resource = ResourceHelper.CreateStream(newValue);

                // todo - fix put-async
                _settingProvider.PutAsync(uri, resource.Stream, PopulateProviderInfo(settingMetadata, ResourceMetadata.Empty.Format(resource.Format))).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Assigns the same setting value to the specified member.
        /// </summary>
        public void BindSetting<T>([NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);
            var value = GetSetting(expression, instanceName);
            settingMetadata.SetValue(Validate(value, settingMetadata.Validations, uri));
        }

        /// <summary>
        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        /// </summary>        
        public void BindSettings<T>([NotNull] T obj, [CanBeNull] string instanceName = null)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var settingProperties =
                typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.IsDefined(typeof(SettingMemberAttribute)));

            foreach (var property in settingProperties)
            {
                // This expression allows to reuse GeAsync.
                var expression = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(obj),
                        property.Name
                    )
                );

                var value = GetSetting(expression, instanceName);
                var settingMetadata = SettingMetadata.FromExpression(expression, false);
                var uri = settingMetadata.CreateUri(instanceName);
                settingMetadata.SetValue(Validate(value, settingMetadata.Validations, uri));
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

        private static ResourceMetadata PopulateProviderInfo(SettingMetadata settingMetadata, ResourceMetadata metadata = null)
        {
            return
                (metadata ?? ResourceMetadata.Empty)
                .Add(ResourceMetadataKeys.ProviderCustomName, settingMetadata.ProviderName)
                .Add(ResourceMetadataKeys.ProviderDefaultName, settingMetadata.ProviderType?.ToPrettyString());
        }

        // --------

        private ITypeConverter _converter = TypeConverter.Empty;

        private JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                new StringEnumConverter(),
                new Reusable.Utilities.JsonNet.ColorConverter()
            }
        };

        private IImmutableSet<Type> _stringTypes = new[]
            {
                typeof(string),
                typeof(Enum),
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(Color)
            }
            .ToImmutableHashSet();

        private ITypeConverter GetOrAddDeserializer(Type toType)
        {
            if (_converter.CanConvert(typeof(string), toType))
            {
                return _converter;
            }

            var converter = CreateJsonConverter(typeof(JsonToObjectConverter<>), toType);
            return (_converter = _converter.Add(converter));
        }

        private ITypeConverter GetOrAddSerializer(Type fromType)
        {
            if (_converter.CanConvert(fromType, typeof(string)))
            {
                return _converter;
            }

            var converter = CreateJsonConverter(typeof(ObjectToJsonConverter<>), fromType);
            return (_converter = _converter.Add(converter));
        }

        private ITypeConverter CreateJsonConverter(Type converterType, Type valueType)
        {
            var converterGenericType = converterType.MakeGenericType(valueType);
            return (ITypeConverter)Activator.CreateInstance(converterGenericType, _settings, _stringTypes);
        }

        #endregion
    }
}