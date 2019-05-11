using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Utilities.JsonNet.Converters
{
    public class ListConverter<TItemBase> : JsonConverter
    {
        // ReSharper disable once StaticMemberInGenericType - We want this to be static for the current type.
        private static readonly IDictionary<SoftString, Type> Types;

        static ListConverter()
        {
            Types =
                typeof(TItemBase)
                    .Assembly
                    .GetTypes()
                    .Where(t => t.IsClass && typeof(TItemBase).IsAssignableFrom(t))
                    .ToDictionary(t => t.Name.ToSoftString());
        }

        public override bool CanConvert(Type objectType)
        {
            return
                objectType.IsGenericType &&
                objectType.GetGenericTypeDefinition().In(
                    typeof(IEnumerable<>),
                    typeof(IList<>),
                    typeof(List<>)
                );
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
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

                        var typeName =
                            typeToken.SelectToken("$.$t").Value<string>()
                            ?? throw DynamicException.Create("TypeNameNotFound", $"Type name is missing at '{reader.Path}'.");

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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("Not required in this scenario because it's only for reading.");
        }
    }
}