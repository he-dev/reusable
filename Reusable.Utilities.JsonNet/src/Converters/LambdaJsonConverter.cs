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
        [CanBeNull]
        public Func<string, T> ReadJsonCallback { get; set; }

        [CanBeNull]
        public Func<T, string> WriteJsonCallback { get; set; }

        public override bool CanRead => !(ReadJsonCallback is null);

        public override bool CanWrite => !(WriteJsonCallback is null);

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var value = jToken.Value<string>();

            return ReadJsonCallback(value);
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, WriteJsonCallback(value));
        }
    }
}