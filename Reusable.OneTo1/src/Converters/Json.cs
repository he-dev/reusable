using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.OneTo1.Converters
{
    [PublicAPI]
    public abstract class JsonConverter : ITypeConverter
    {
        // String-types require quotes for serialization.
        public StringTypeCollection StringTypes { get; } = new StringTypeCollection
        {
            typeof(string),
            typeof(bool),
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
    }

    public class StringTypeCollection : IEnumerable<Type>
    {
        private readonly ISet<Type> _types = new HashSet<Type>();

        public void Add(Type type) => _types.Add(type);

        public bool Contains(Type type) => _types.Contains(type.IsEnum ? typeof(Enum) : type);

        public IEnumerator<Type> GetEnumerator() => _types.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_types).GetEnumerator();
    }

    public class JsonToObject : JsonConverter
    {
        public override object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return
                value is string json
                    ? JsonConvert.DeserializeObject(QuoteOrDefault(json, toType), toType, Settings)
                    : default;
        }

        private string QuoteOrDefault(string json, Type toType)
        {
            return StringTypes.Contains(toType) ? $@"""{json.Trim('"')}""" : json;
        }
    }

    public class ObjectToJson : JsonConverter
    {
        public override object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return
                toType == typeof(string)
                    ? JsonConvert.SerializeObject(value, Settings).Map<string, string>(json => json.Trim('"'))
                    : default(object);
        }
    }
}