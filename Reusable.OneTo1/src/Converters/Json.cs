using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.OneTo1.Converters
{
    [PublicAPI]
    public abstract class JsonConverter : ITypeConverter
    {
        // String-types require quotes for serialization.
        public ISet<Type> StringTypes { get; } = new HashSet<Type>
        {
            typeof(string),
            typeof(Enum),
            typeof(TimeSpan),
            typeof(DateTime),
            typeof(Color),
        };

        public JsonSerializerSettings Settings { get; set; } = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        public abstract object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default);

        protected static bool IsQuoted(string text)
        {
            return text.StartsWith("\"") && text.EndsWith("\"");
        }

        protected bool IsStringType(Type type)
        {
            return StringTypes.Contains(type.IsEnum ? typeof(Enum) : type);
        }
    }

    public class JsonToObject : JsonConverter
    {
        public override object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            if (value is string json)
            {
                json = IsStringType(toType) && !IsQuoted(json) ? Quote(json) : json;

                return JsonConvert.DeserializeObject(json, toType, Settings);
            }
            else
            {
                return default;
            }
        }

        private static string Quote(string value)
        {
            return $@"""{value}""";
        }
    }

    public class ObjectToJson : JsonConverter
    {
        public override object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            if (toType == typeof(string))
            {
                var result = JsonConvert.SerializeObject(value, Settings);

                // String-types must not contain quotes after serialization.
                return
                    IsStringType(toType)
                        ? Unquote(result)
                        : result;
            }
            else
            {
                return default;
            }
        }

        private static string Unquote(string value)
        {
            return value.Trim('"');
        }
    }
}