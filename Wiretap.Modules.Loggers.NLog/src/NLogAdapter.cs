using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using Reusable.Extensions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Modules.Loggers;

public class NLogAdapter : ILogger
{
    public IDictionary<Severity, LogLevel> Levels { get; set; } = new Dictionary<Severity, LogLevel>()
    {
        { Severity.High, LogLevel.Error },
        { Severity.Normal, LogLevel.Info },
        { Severity.Low, LogLevel.Debug },
    };

    private NLog.Logger? Logger { get; set; }

    public void Log(IEnumerable<LogProperty> properties)
    {
        var logEventInfo = new LogEventInfo
        {
            Level = LogLevel.Info
        };

        foreach (var property in properties)
        {
            switch (property.Name)
            {
                case "trace":
                    if (property.Value is Trace t)
                    {
                        logEventInfo.Message = t.Message;
                        logEventInfo.LoggerName = t.Name;
                    }

                    logEventInfo.Properties.Add(property.Name, property.Value);

                    break;

                case "severity":
                    logEventInfo.Level = Levels[(Severity)property.Value!];
                    break;

                //case "exception":
                //    logEventInfo.Exception = (Exception)property.Value!;
                //    break;

                default:
                    if (property.Value is { } value)
                    {
                        logEventInfo.Properties.Add(property.Name, value);
                    }

                    break;
            }
        }

        (Logger ??= LogManager.GetLogger("Wiretap")).Log(logEventInfo);
    }
}

public class NativeJsonSerializer(JsonSerializerOptions options) : IJsonConverter
{
    public static readonly IJsonConverter Default = new NativeJsonSerializer(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new asdf()
        }
    });

    public bool SerializeObject(object value, StringBuilder builder)
    {
        try
        {
            builder.Append(JsonSerializer.Serialize(value, options));
            return true;
        }
        catch (Exception e)
        {
            NLog.Common.InternalLogger.Error(e, $"Error serializing '{value.GetType()}' at '{builder}'.");
            return false;
        }
    }

    public static void Register(IJsonConverter? jsonConverter = default)
    {
        LogManager.Setup().SetupSerialization(s => s.RegisterJsonConverter(jsonConverter ?? Default));
    }
}

public class asdf : JsonConverter<Exception>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exception).IsAssignableFrom(typeToConvert);
    }

    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}