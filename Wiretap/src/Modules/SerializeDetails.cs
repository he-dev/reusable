using System.Text.Json;
using System.Text.Json.Serialization;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class SerializeDetails : IModule
{
    public JsonSerializerOptions Options { get; set; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters =
        {
            //new StringEnumConverter(),
            //new SoftStringConverter(),
            //new KeyValuePairConverter<SoftString, object>()
            new JsonStringEnumConverter()
        },
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    public void Invoke(TraceContext context, LogFunc next)
    {
        context.Entry.SetItem(Strings.Items.Details, JsonSerializer.Serialize(context.Entry.Details(), Options));
        next(context);
    }
}