using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Reusable.Apps.Server.Json
{
    public class JsonStringConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JsonString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);
            return new JsonString(jToken.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
