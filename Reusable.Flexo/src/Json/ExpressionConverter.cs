using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Json
{
    // Converts a value into Constant when it's not an Expression.
    public class ExpressionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface && typeof(IExpression).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{nameof(ExpressionConverter)} does not support writing.");
        }

        private int _index;

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value is null && reader.TokenType == JsonToken.StartObject)
            {
                _index = 0;
                var jToken = JObject.Load(reader);
                var typeName = (string?)jToken.SelectToken("$.$type") ?? throw DynamicException.Create("TypePropertyNotFound", $"Could not find '$type' property on '{jToken}'.");
                var actualType = Type.GetType(typeName) ?? throw DynamicException.Create("TypeNotFound", $"Could not find type '{typeName}'.");
                return jToken.ToObject(actualType, serializer);
            }
            else
            {
                var jTokenReader = (JTokenReader)reader;
                var name =
                    jTokenReader.CurrentToken.Parent is JArray
                        ? $"Item[{_index++}]"
                        : GetParentName(jTokenReader.CurrentToken.Parent);

                return CreateConstant(name, reader.Value);
            }
        }

        private static string GetParentName(JContainer? jContainer)
        {
            return jContainer switch
            {
                JProperty p => p.Name,
                JArray a => GetParentName(a.Parent),
                _ => "Value"
            };
        }

        private static IConstant CreateConstant(string name, object? value)
        {
            return value switch
            {
                double d => (IConstant)new Double(name, d),
                bool b => (IConstant)Constant.FromValue(name, b),
                {} x => (IConstant)Activator.CreateInstance(MakeGenericConstant(x), name, x, default),
                _ => throw new InvalidOperationException($"{name} must not be null.")
            };

            Type MakeGenericConstant(object x) => typeof(Constant<>).MakeGenericType(x.GetType());
        }
    }
}