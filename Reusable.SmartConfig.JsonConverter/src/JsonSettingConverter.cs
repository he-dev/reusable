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
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    /*
     *
     * This converter is build on top of the Reusable.Converters.
     * It initializes each json-converter on demand.
     * 
     */

    [PublicAPI]
    public class JsonResourceProvider : ConvertedResourceProvider
    {
        [NotNull] private ITypeConverter _converter;

        [NotNull] private JsonSerializerSettings _settings;

        [NotNull] private IImmutableSet<Type> _stringTypes;

        public JsonResourceProvider(IResourceProvider resourceProvider) : base(resourceProvider, typeof(string))
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

        public static Func<IResourceProvider, IResourceProvider> Factory() => dercorable => new JsonResourceProvider(dercorable);

        public override async Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            var info = await ResourceProvider.GetAsync(uri);
            if (info.Exists)
            {
                var value = await info.DeserializeAsync(typeof(string));
                return new JsonResourceInfo
                (
                    uri,
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

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream stream, ResourceMetadata metadata = null)
        {
            if (metadata.TryGetValue("StreamType", out string type) && type != "Object")
            {
                throw new ArgumentException(paramName: nameof(stream), message: "Can post only stream of object type.");
            }

            if (metadata.TryGetValue(ResourceMetadataKeys.Serializer, out string serializerName))
            {
                var value = default(object);

                if (serializerName == nameof(BinaryFormatter))
                {
                    var binaryFormatter = new BinaryFormatter();
                    using (var memoryStream = new MemoryStream())
                    {
                         value = binaryFormatter.Deserialize(memoryStream);
                    }
                }

                if (serializerName == nameof(StreamReader))
                {
                    //stream.Seek(0, SeekOrigin.Begin);
                    using (var streamReader = new StreamReader(stream))
                    {
                        value = await streamReader.ReadToEndAsync();
                    }
                }

                if (SupportedTypes.Contains(value.GetType()))
                {
                    var fromType = stream.GetType();
                    var serialized = (string)GetOrAddSerializer(fromType).Convert(value, typeof(string));

                    using (var streamReader = serialized.ToStreamReader())
                    {
                        return await ResourceProvider.PutAsync(uri, streamReader.BaseStream);
                    }
                }

                throw DynamicException.Create("UnsupportedSerializer", $"Cannot deserialize '{uri}' because the serializer '{serializerName}' is not supported.");
            }

            throw new Exception("Blub");
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
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

    internal class JsonResourceInfo : ResourceInfo
    {
        [CanBeNull]
        private readonly object _value;

        private readonly Func<Type, ITypeConverter> _getOrAddConverter;

        internal JsonResourceInfo
        (
            [NotNull] UriString uri,
            [CanBeNull] object value,
            Func<Type, ITypeConverter> getOrAddConverter
        )
            : base(uri)
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