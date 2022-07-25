using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Middleware;

[PublicAPI]
public class SerializeToJson : SerializeProperty
{
    public SerializeToJson(string propertyName) : base(propertyName) { }

    public JsonSerializerOptions Options { get; set; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            //new StringEnumConverter(),
            //new SoftStringConverter(),
            //new KeyValuePairConverter<SoftString, object>()
            new JsonStringEnumConverter()
        },
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    protected override object Serialize(object value)
    {
        return value is string ? value : JsonSerializer.Serialize(value, Options);
    }
}