using System.Text.Json;
using System.Text.Json.Serialization;
using Reusable.Extensions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Filters;

public class SerializeSnapshot : IModule
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

    public void Invoke(TraceContext context, LogAction next)
    {
        context.Entry.SetItem(Strings.Items.Snapshot, JsonSerializer.Serialize(context.Entry.Details(), Options));
        next(context);
    }
}