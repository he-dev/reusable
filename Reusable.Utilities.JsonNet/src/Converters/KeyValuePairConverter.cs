using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Converters;

// Json.net will serialize 'IEnumerable<KeyValuePair<TKey, TValue>>' by default as { 'Key': 'key', 'Value': 'value' }
// This converter uses the actual key/value pairs as property/value rather then serializing the container object: { 'key': 'value' }
public class KeyValuePairConverter<TKey, TValue> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(IEnumerable<KeyValuePair<TKey, TValue>>).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var items = value as IEnumerable<KeyValuePair<TKey, TValue>> ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>();

        writer.WriteStartObject();
        foreach (var item in items)
        {
            writer.WritePropertyName(item.Key.ToString());
            //writer.WriteValue(item.Value);
            serializer.Serialize(writer, item.Value);
        }
        writer.WriteEndObject();
    }
}