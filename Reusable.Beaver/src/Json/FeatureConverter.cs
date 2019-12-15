using System;
using Newtonsoft.Json;
using Reusable.Exceptionize;

namespace Reusable.Beaver.Json
{
    /// <summary>
    /// Converts string into Feature.
    /// </summary>
    public class FeatureConverter : JsonConverter
    {
        // Helps to prevent recursive ReadJson call
        private bool isConverting;

        public override bool CanConvert(Type objectType)
        {
            return !isConverting && (isConverting = objectType == typeof(Feature));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
            {
                JsonToken.EndObject => DeserializeFeature(reader, serializer),
                JsonToken.String => new Feature((string)reader.Value),
                _ => throw DynamicException.Create("FeatureConversion", $"Cannot deserialize '{reader.TokenType}' into a feature.")
            };
        }

        private Feature DeserializeFeature(JsonReader reader, JsonSerializer serializer)
        {
            try
            {
                return serializer.Deserialize<Feature>(reader);
            }
            finally
            {
                isConverting = false;
            }
        }


        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{nameof(FeatureConverter)} does not support writing.");
        }
    }
}