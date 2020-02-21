using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.OneTo1.Converters.Generic
{
    [PublicAPI]
    public abstract class JsonConverter : TypeConverter
    {
        protected ISet<Type> StringTypes { get; } = new HashSet<Type>
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
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(string);
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            var json = value as string;

            // String-types require quotes for deserialization.
            var requiresQuotes = IsStringType(toType) && !IsQuoted(json);

            return JsonConvert.DeserializeObject(requiresQuotes ? Quote(json) : json, toType, Settings);
        }

        private static string Quote(string value)
        {
            return $@"""{value}""";
        }
    }

    public class ObjectToJson : JsonConverter
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return toType == typeof(string);
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            var result = JsonConvert.SerializeObject(value, Settings);

            // String-types must not contain quotes after serialization.
            return
                IsStringType(toType)
                    ? Unquote(result)
                    : result;
        }

        private static string Unquote(string value)
        {
            return value.Trim('"');
        }
    }
}