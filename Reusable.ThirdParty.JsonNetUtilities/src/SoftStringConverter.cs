using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Drawing;
using Reusable.Extensions;
using Reusable.Formatters;

namespace Reusable.Utilities.ThirdParty.JsonNet
{
    [PublicAPI]
    public class SoftStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SoftString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var softString = jToken.Value<string>();
            return SoftString.Create(softString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}