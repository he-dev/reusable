using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq.Custom;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Convertia;
using Reusable.Convertia.Converters;
using Reusable.Convertia.Converters.Generic;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Reusable.Stratus;

namespace Reusable.SmartConfig
{
    /*
     *
     * This converter is build on top of the Reusable.Converters.
     * It initializes each json-converter on demand.
     * 
     */

    [PublicAPI]
    public class JsonValueProvider : ConvertedValueProvider
    {
        [NotNull] private ITypeConverter _converter;

        [NotNull] private JsonSerializerSettings _settings;

        [NotNull] private IImmutableSet<Type> _stringTypes;

        public JsonValueProvider(Stratus.IValueProvider valueProvider) : base(valueProvider, typeof(string))
        {
            _converter = TypeConverter.Empty;
            _settings = new JsonSerializerSettings();
            _stringTypes = ImmutableHashSet<Type>.Empty;
        }

        [NotNull]
        public static readonly JsonSettingConverter Default = new JsonSettingConverter
        {
            Settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters =
                {
                    new StringEnumConverter(),
                    new Reusable.Utilities.JsonNet.ColorConverter()
                }
            },
            StringTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(Enum),
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(Color)
            },
        };

        [NotNull]
        public JsonSerializerSettings Settings
        {
            get => _settings;
            set => _settings = value ?? throw new ArgumentNullException(nameof(Settings));
        }

        [NotNull, ItemNotNull]
        public IImmutableSet<Type> StringTypes
        {
            get => _stringTypes;
            set => _stringTypes = value ?? throw new ArgumentNullException(nameof(StringTypes));
        }

        public static Func<Stratus.IValueProvider, Stratus.IValueProvider> Factory() => dercorable => new JsonValueProvider(dercorable);

        public override async Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            var info = await ValueProvider.GetValueInfoAsync(name);
            if (info.Exists)
            {
                var value = await info.DeserializeAsync(typeof(string));
                return new JsonValueInfo
                (
                    name,
                    value,
                    targetType =>
                        _converter.CanConvert(typeof(string), targetType)
                            ? _converter
                            : _converter = _converter.Add(GetOrAddDeserializer(targetType))
                );
            }
            else
            {
                return info;
            }
        }

        public override async Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (SupportedTypes.Contains(value.GetType()))
            {
                var fromType = value.GetType();
                var serialized = (string)GetOrAddSerializer(fromType).Convert(value, typeof(string));

                return await ValueProvider.SerializeAsync(name, serialized);
            }

            throw new Exception();
        }

        public override async Task<IValueInfo> SerializeAsync(string name, Stream value, ValueProviderMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(value))
            {
                return await SerializeAsync(name, await valueReader.ReadToEndAsync());
            }
        }

        public override Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            throw new NotImplementedException();
        }

        #region Helpers

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
            return (ITypeConverter)Activator.CreateInstance(converterGenericType, _settings, StringTypes);
        }

        #endregion
    }

    internal class JsonValueInfo : ValueInfo
    {
        [CanBeNull]
        private readonly object _value;

        private readonly Func<Type, ITypeConverter> _getOrAddConverter;

        internal JsonValueInfo
        (
            [NotNull] string name,
            [CanBeNull] object value,
            Func<Type, ITypeConverter> getOrAddConverter
        )
            : base(name)
        {
            _value = value;
            _getOrAddConverter = getOrAddConverter;
        }

        public override bool Exists => !(_value is null);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        public override Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
                new BinaryFormatter().Serialize(stream, _value);
            }

            return Task.CompletedTask;
        }

        public override Task<object> DeserializeAsync(Type targetType)
        {
            var converter = _getOrAddConverter(targetType);
            return Task.FromResult(converter.Convert(_value, targetType));
        }
    }

















    [PublicAPI]
    public class JsonSettingConverter : SettingConverter
    {
        [NotNull] private ITypeConverter _converter;

        [NotNull] private JsonSerializerSettings _settings;

        [NotNull] private ISet<Type> _stringTypes;

        public JsonSettingConverter() : base(new Type[0], typeof(string))
        {
            _converter = TypeConverter.Empty;
            _settings = new JsonSerializerSettings();
            _stringTypes = new HashSet<Type>();
        }

        [NotNull]
        public static readonly JsonSettingConverter Default = new JsonSettingConverter
        {
            Settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters =
                {
                    new StringEnumConverter(),
                    new Reusable.Utilities.JsonNet.ColorConverter()
                }
            },
            StringTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(Enum),
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(Color)
            },
        };

        [NotNull]
        public JsonSerializerSettings Settings
        {
            get => _settings;
            set => _settings = value ?? throw new ArgumentNullException(nameof(Settings));
        }

        [NotNull, ItemNotNull]
        public ISet<Type> StringTypes
        {
            get => _stringTypes;
            set => _stringTypes = value ?? throw new ArgumentNullException(nameof(StringTypes));
        }

        protected override object DeserializeCore(object value, Type targetType)
        {
            if (!(value is string)) throw new ArgumentException($"Unsupported type '{targetType.ToPrettyString()}'. Only {typeof(string).ToPrettyString()} is allowed.");

            return GetOrAddDeserializer(targetType).Convert(value, targetType);
        }

        protected override object SerializeCore(object value, Type targetType)
        {
            var fromType = value.GetType();
            return (string)GetOrAddSerializer(fromType).Convert(value, targetType);
        }

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
            return (ITypeConverter)Activator.CreateInstance(converterGenericType, _settings, StringTypes);
        }
    }
}