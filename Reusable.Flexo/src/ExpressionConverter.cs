using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    // Converts a value into Constant when it's not an Expression.
    public class ExpressionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface && objectType == typeof(IExpression);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{nameof(ExpressionConverter)} does not support writing.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is null && reader.TokenType == JsonToken.StartObject)
            {
                var jToken = JObject.Load(reader);
                var typeName = (string)jToken.SelectToken("$.$type");

                if (typeName is null)
                {
                    throw DynamicException.Create("TypePropertyNotFound", $"Could not find '$type' property on '{jToken}'.");
                }

                var actualType = Type.GetType(typeName);
                var result = jToken.ToObject(actualType, serializer);
                return (IExpression)result;
            }
            else
            {
                var name = GetParentName(((JTokenReader)reader).CurrentToken.Parent);
                return Constant.FromValue(name, reader.Value);
            }
        }

        private static string GetParentName(JContainer jContainer)
        {
            switch (jContainer)
            {
                case JProperty jProperty: return jProperty.Name;
                case JArray jArray: return GetParentName(jArray.Parent);
            }

            return "Value";
        }
    }
}