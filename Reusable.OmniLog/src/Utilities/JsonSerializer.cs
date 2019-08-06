﻿using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.OmniLog.Utilities
{
    public class JsonSerializer : ISerializer
    {
        [NotNull]
        public JsonSerializerSettings Settings { get; set; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            Converters =
            {
                new StringEnumConverter(),
                new SoftStringConverter(),
                new KeyValuePairConverter<SoftString, object>()
            }
        };

        public object Serialize(object obj)
        {
            //return obj is string ? obj : JsonConvert.SerializeObject(obj, Settings);
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}