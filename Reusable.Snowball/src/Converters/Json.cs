using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Essentials;

namespace Reusable.Snowball.Converters;

[PublicAPI]
public abstract class JsonConverter : ITypeConverter
{
    // String-types require quotes for serialization.
    public StringTypeCollection StringTypes { get; } = new()
    {
        typeof(string),
        typeof(bool),
        typeof(Enum),
        typeof(TimeSpan),
        typeof(DateTime),
        typeof(Color),
    };

    public JsonSerializerSettings Settings { get; set; } = new()
    {
        Culture = CultureInfo.InvariantCulture,
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
    };

    public abstract object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default);
}

public class StringTypeCollection : IEnumerable<Type>
{
    private ISet<Type> Types { get; } = new HashSet<Type>();

    public void Add(Type type) => Types.Add(type);

    public bool Contains(Type type) => Types.Contains(type.IsEnum ? typeof(Enum) : type);

    public IEnumerator<Type> GetEnumerator() => Types.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Types).GetEnumerator();
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
                ? JsonConvert.SerializeObject(value, Settings).Map(json => json.Trim('"'))
                : default(object);
    }
}