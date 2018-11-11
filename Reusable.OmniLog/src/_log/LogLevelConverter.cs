using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Reusable.OmniLog
{
    public class LogLevelConverter : JsonConverter<LogLevel>
    {
        public override void WriteJson(JsonWriter writer, LogLevel value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override LogLevel ReadJson(JsonReader reader, Type objectType, LogLevel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var logLevelString = jToken.Value<string>();
            return LogLevel.Parse(logLevelString);
        }
    }
}