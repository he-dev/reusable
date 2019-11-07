using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters.Generic;
using ColorConverter = Reusable.Utilities.JsonNet.Converters.ColorConverter;

namespace Reusable.Translucent.Converters
{
    [PublicAPI]
    public class JsonSettingConverter : ITypeConverter
    {
        public static JsonSerializerSettings DefaultSerializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                new StringEnumConverter(),
                new ColorConverter()
            }
        };

        public static IImmutableSet<Type> DefaultStringTypes
        {
            get
            {
                return
                    ImmutableHashSet<Type>
                        .Empty
                        .Add(typeof(string))
                        .Add(typeof(Enum))
                        .Add(typeof(TimeSpan))
                        .Add(typeof(DateTime))
                        .Add(typeof(Color));
            }
        }

        private readonly JsonSerializerSettings _settings;

        private readonly IImmutableSet<Type> _stringTypes;

        private ITypeConverter _converter;

        public JsonSettingConverter(Func<JsonSerializerSettings, JsonSerializerSettings> configureSerializerSettings, IEnumerable<Type> stringTypes)
        {
            _settings = configureSerializerSettings(DefaultSerializerSettings);
            _stringTypes = stringTypes.ToImmutableHashSet();
            _converter = TypeConverter.Empty;
        }
        
        public JsonSettingConverter(Action<JsonSerializerSettings> configureSerializerSettings, IEnumerable<Type> stringTypes)
        {
            _settings = DefaultSerializerSettings;
            configureSerializerSettings?.Invoke(_settings);
            _stringTypes = stringTypes.ToImmutableHashSet();
            _converter = TypeConverter.Empty;
        }

        public JsonSettingConverter() : this(x => x, DefaultStringTypes) { }

        public Type FromType => throw new NotSupportedException();

        public Type ToType => throw new NotSupportedException();

        public bool TrimDoubleQuotes { get; set; }

        public bool CanConvert(Type fromType, Type toType)
        {
            return _converter.CanConvert(fromType, toType);
        }

        public object Convert(IConversionContext<object> context)
        {
            if (context.FromType == typeof(string) && context.ToType == typeof(string))
            {
                return TrimDoubleQuotes ? ((string)context.Value).Trim('"') : context.Value;
            }

            if (CanConvert(context.FromType, context.ToType))
            {
                return _converter.Convert(context);
            }
            else
            {
                var newConverterType = CreateJsonConverterType(context);
                var newConverter = (ITypeConverter)Activator.CreateInstance(newConverterType, _settings, _stringTypes);
                _converter = _converter.Add(newConverter);
                return Convert(context);
            }
        }

        public bool Equals(ITypeConverter other) => throw new NotSupportedException();

        private Type CreateJsonConverterType(IConversionContext<object> context)
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