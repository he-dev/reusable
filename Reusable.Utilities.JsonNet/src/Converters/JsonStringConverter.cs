using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Utilities.JsonNet.Converters
{
    [PublicAPI]
    public class JsonStringConverter : JsonConverter
    {
        private readonly IImmutableSet<Type> _stringTypes;

        public JsonStringConverter(params Type[] stringTypes)
        {
            _stringTypes = stringTypes.ToImmutableHashSet();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetCustomAttributes<JsonStringAttribute>().Any() || _stringTypes.Contains(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var value = jToken.Value<string>();

            if (objectType.GetConstructor(new[] { typeof(string) }) is var constructor && !(constructor is null))
            {
                return constructor.Invoke(new object[] { value });
            }

            if (objectType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static) is var parse && !(parse is null))
            {
                return parse.Invoke(null, new object[] { value });
            }

            throw DynamicException.Create
            (
                "CannotCreateObject",
                $"{objectType.ToPrettyString()} is decorated with the {nameof(JsonStringAttribute)} so it must either have a constructor with a string parameter or a static Parse method."
            );
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}