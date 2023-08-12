using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

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

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (entry.TryGetValue(LogEntry.PropertyNames.Details, out var value) && value is not null)
        {
            entry = entry.SetItem(LogEntry.PropertyNames.Details, JsonSerializer.Serialize(value, Options));
        }
        else
        {
            entry = entry.RemoveItem(LogEntry.PropertyNames.Details);
        }

        next(activity, entry);
    }
}