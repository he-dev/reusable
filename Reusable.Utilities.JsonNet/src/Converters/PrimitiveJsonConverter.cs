using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet.Converters
{
    [UsedImplicitly]
    public class PrimitiveJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var primitiveType = GetPrimitiveType(objectType);
            return primitiveType != null && MakePrimitiveType(primitiveType).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var primitiveType = GetPrimitiveType(value.GetType());
            // ReSharper disable once PossibleNullReferenceException - CanConvert guards this so nothing is null here.
            writer.WriteValue(
                MakePrimitiveType(primitiveType)
                    .GetProperty(nameof(Primitive<object>.Value), BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(value)
            );
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var primitiveType = GetPrimitiveType(objectType);
            // ReSharper disable once PossibleNullReferenceException - CanConvert guards this so nothing is null here.
            return
                objectType
                    .GetConstructor(new[] { primitiveType })
                    // JsonNet reads int as int64 so downgrade it here if necessary.
                    .Invoke(new[] { Convert.ChangeType(reader.Value, primitiveType) });
        }

        private static Type GetPrimitiveType(Type objectType)
        {
            // ReSharper disable once PossibleNullReferenceException - I doubt BaseType is ever null.
            return objectType.BaseType.GetGenericArguments().FirstOrDefault();
        }

        private static Type MakePrimitiveType(Type primitiveType)
        {
            return typeof(Primitive<>).MakeGenericType(primitiveType);
        }
    }
}