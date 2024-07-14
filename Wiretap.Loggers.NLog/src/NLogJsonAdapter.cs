using System;
using System.Text;
using System.Text.Json;
using NLog;
using Reusable.Wiretap.Json.Converters;

namespace Reusable.Wiretap.Loggers;

public class NLogJsonAdapter : IJsonConverter
{
    public JsonSerializerOptions Options { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new DataConverter(),
            new ExceptionConverter()
        }
    };

    public bool SerializeObject(object value, StringBuilder builder)
    {
        try
        {
            builder.Append(JsonSerializer.Serialize(value, Options));
            return true;
        }
        catch (Exception e)
        {
            global::NLog.Common.InternalLogger.Error(e, $"Error serializing '{value.GetType()}' at '{builder}'.");
            return false;
        }
    }

    public static void Register(Action<JsonSerializerOptions>? configure = default)
    {
        LogManager
            .Setup()
            .SetupSerialization
            (
                s =>
                {
                    var jsonConverter = new NLogJsonAdapter();
                    configure?.Invoke(jsonConverter.Options);
                    s.RegisterJsonConverter(jsonConverter);
                });
    }
}