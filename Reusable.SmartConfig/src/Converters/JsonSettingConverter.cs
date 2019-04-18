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

namespace Reusable.SmartConfig
{
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