using System;
using Newtonsoft.Json;
using Reusable.Exceptionize;
using Reusable.Utilities.JsonNet;

namespace Reusable.Beaver.Json
{
    public class FeaturePolicyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface && typeof(IFeaturePolicy).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
            {
                JsonToken.EndObject => reader.ToObject<IFeaturePolicy>(reader.GetJsonType(), serializer),
                JsonToken.Boolean => ((bool)reader.Value ? FeaturePolicy.AlwaysOn : FeaturePolicy.AlwaysOff),
                _ => throw DynamicException.Create("FeaturePolicyConversion", $"Cannot deserialize '{reader.TokenType}' into a feature-policy.")
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{nameof(FeaturePolicyConverter)} does not support writing.");
        }
    }
}