using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reusable.Wiretap.Json.Converters;

public class DataConverter : JsonConverter<DataCollection>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(DataCollection).IsAssignableFrom(typeToConvert);
    }

    public override DataCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, DataCollection value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        var properties = new HashSet<string>();
        foreach (var obj in value)
        {
            using var jsonDocument = JsonSerializer.SerializeToDocument(obj, options);
            var jsonElements =
                from jsonElement in jsonDocument.RootElement.EnumerateObject()
                where properties.Add(jsonElement.Name)
                select jsonElement;

            foreach (var jsonElement in jsonElements)
            {
                jsonElement.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}