using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.SmartConfig.Services
{
    public class JsonSettingConverter : SettingConverter
    {
        public JsonSettingConverter(JsonSerializerSettings settings = null)
        {
            JsonSerializerSettings = settings ?? new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        [NotNull]
        public JsonSerializerSettings JsonSerializerSettings { get; }
        protected override T DeserializeCore<T>(object value)
        {
            if (value is string s)
            {
                return JsonConvert.DeserializeObject<T>(s, JsonSerializerSettings);
            }
            throw new ArgumentException($"Unsupported type '{typeof(T).Name}'.");
        }

        protected override object SerializeCore(object value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }
    }
}