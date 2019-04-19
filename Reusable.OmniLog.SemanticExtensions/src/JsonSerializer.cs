using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.OmniLog.SemanticExtensions
{
    public class JsonSerializer : ISerializer
    {
        public JsonSerializer()
        {
            Settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.None,
                Converters =
                {
                    new StringEnumConverter(),
                    new SoftStringConverter(),
                    new KeyValuePairConverter<SoftString, object>()
                }
            };
        }

        [NotNull]
        public JsonSerializerSettings Settings { get; set; }

        public object Serialize(object obj)
        {
            //return obj is string ? obj : JsonConvert.SerializeObject(obj, Settings);
            return  JsonConvert.SerializeObject(obj, Settings);
        }
    }
}