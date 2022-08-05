using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Marbles.Reflection;

namespace Reusable.Utilities.JsonNet.Converters;

public class ListConverter<T> : JsonConverter
{
    public ListConverter(string typePropertyName = "$t")
    {
        TypePropertyName = typePropertyName;
        Types = Reflect<T>.GetTypesAssignableFrom().ToImmutableDictionary(t => t.ToPrettyString());
    }
        
    public string TypePropertyName { get; }

    public IImmutableDictionary<string, Type> Types { get; set; }

    public override bool CanConvert(Type objectType)
    {
        return
            objectType.IsGenericType &&
            objectType.GetInterfaces().Any(i => i == typeof(IList<T>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var valueType = objectType.GetGenericArguments()[0];
        var collectionType = typeof(List<>).MakeGenericType(valueType);
        var collection = (IList)Activator.CreateInstance(collectionType);

        // Save depth to know when to stop.
        var arrayDepth = reader.Depth;
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                {
                    var typeToken = JToken.ReadFrom(reader);
                    var typeName = typeToken.SelectToken($"$.{TypePropertyName}")?.Value<string>() ?? throw DynamicException.Create("TypeNameNotFound", $"Type name is missing at '{reader.Path}'.");
                    var obj = serializer.Deserialize(typeToken.CreateReader(), Types[typeName]);
                    collection.Add(obj);
                }
                    break;

                case JsonToken.EndArray when reader.Depth == arrayDepth:
                    // We've reached the end of the array.
                    return collection;
            }
        }

        return collection;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotSupportedException("Not required in this scenario because it's only for reading.");
    }
}