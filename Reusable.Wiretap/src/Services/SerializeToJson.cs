using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Reusable.Essentials;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Services;

public class SerializeToJson : ISerialize
{
    public JsonSerializerSettings Settings { get; set; } = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
        Converters =
        {
            new StringEnumConverter(),
            new SoftStringConverter(),
            new KeyValuePairConverter<SoftString, object>()
        },
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };

    public object Invoke(object value)
    {
        return value is string ? value : JsonConvert.SerializeObject(value, Settings);
    }
}