using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Reusable.OmniLog.SemanticExtensions
{
    public class JsonStateSerializer : IStateSerializer
    {
        public JsonStateSerializer()
        {
            Settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };
        }

        [NotNull]
        public JsonSerializerSettings Settings { get; set; }

        public object SerializeObject(object obj)
        {
            //return obj is string ? obj : JsonConvert.SerializeObject(obj, Settings);
            return  JsonConvert.SerializeObject(obj, Settings);
        }
    }
}