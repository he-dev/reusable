using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class SerializeDetails : IMiddleware
{
    public const string Key = "details";

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

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        if (entry.TryGetValue(Key, out var details) && details is IImmutableDictionary<string, object?> { Count: > 0 })
        {
            entry = entry.SetItem(Key, JsonSerializer.Serialize(details, Options));
        }
        else
        {
            entry = entry.RemoveItem(Key);
            
        }

        next(entry);
    }
}