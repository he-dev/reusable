using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Reusable.Utilities.JsonNet.Converters
{
    public delegate object WriteJsonCallback<in T>(T value);

    [PublicAPI]
    public class LambdaJsonConverter<T> : JsonConverter<T>
    {
        public Func<string, T>? ReadJsonCallback { get; set; }

        public Func<T, string>? WriteJsonCallback { get; set; }

        public override bool CanRead => ReadJsonCallback is {};

        public override bool CanWrite => WriteJsonCallback is {};

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (ReadJsonCallback is null) throw new InvalidOperationException($"Cannot read json because {nameof(ReadJsonCallback)} is not set.");
            
            var jToken = JToken.Load(reader);
            var value = jToken.Value<string>();

            return ReadJsonCallback(value);
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (WriteJsonCallback is null) throw new InvalidOperationException($"Cannot write json because {nameof(WriteJsonCallback)} is not set.");
            
            serializer.Serialize(writer, WriteJsonCallback(value));
        }
    }
}