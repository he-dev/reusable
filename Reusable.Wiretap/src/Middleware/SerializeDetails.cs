using System;
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

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        if (entry.TryGetValue(Key, out var details) && details is { })
        {
            entry = entry.SetItem(Key, JsonSerializer.Serialize(details, Options));
        }

        next(entry);
    }
}