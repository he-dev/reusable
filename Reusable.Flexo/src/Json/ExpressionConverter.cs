using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;
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

        //private int _index;

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value is null && reader.TokenType == JsonToken.StartObject)
            {
                //_index = 0;
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
                        ? $"Item"
                        : GetParentName(jTokenReader.CurrentToken.Parent);

                return CreateConstant(name, reader.Value ?? throw new InvalidOperationException($"{name} must not be null."));
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

        private static IConstant CreateConstant(string name, object value)
        {
            return
                (IConstant)typeof(Constant)
                    .GetMethod(nameof(Constant.Single), BindingFlags.Public | BindingFlags.Static)?
                    .MakeGenericMethod(value.GetType())
                    .Invoke(default, new[] { name, value, default })!;
        }
    }
}