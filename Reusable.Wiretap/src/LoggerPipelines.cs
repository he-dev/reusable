using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Nodes.Scopeable;
using Reusable.Wiretap.Services;
using Reusable.Wiretap.Services.Properties;

namespace Reusable.Wiretap;

public static class LoggerPipelines
{
    /// <summary>
    /// Get a pipeline that contains all supported features.
    /// </summary>
    public static IEnumerable<ILoggerNode> Complete
    {
        get
        {
            yield return new InvokePropertyService
            {
                Services = { new Timestamp<DateTimeUtc>() }
            };
            yield return new InvokeEntryAction();
            yield return new GuessProperties
            {
                Factories =
                {
                    new TryGuessProperty(),
                    new TryGuessProperties(),
                    new TryGuessEntryAction(),
                    new TryGuessEntryActions(),
                    new TryGuessEnum(),
                    new TryGuessException(),
                }
            };
            //yield return new Destructure();
            yield return new MapSnapshot();
            yield return new FilterEntries();
            yield return new MapToLogLevel
            {
                Mappers =
                {
                    new MapPropertyToLogLevel
                    {
                        PropertyName = nameof(LoggableProperty.Layer),
                        Mappings =
                        {
                            [nameof(TelemetryLayers.Dependency)] = LogLevel.Trace,
                            [nameof(TelemetryLayers.Application)] = LogLevel.Trace,
                            [nameof(TelemetryLayers.Business)] = LogLevel.Information,
                            [nameof(TelemetryLayers.Presentation)] = LogLevel.Trace
                        }
                    }
                }
            };
            yield return new Fallback
            {
                Properties =
                {
                    [nameof(LoggableProperty.Level)] = LogLevel.Information,
                    [nameof(LoggableProperty.Layer)] = "None",
                    [nameof(LoggableProperty.Category)] = "None",
                    [nameof(LoggableProperty.Unit)] = "None",
                    [nameof(SerializableProperty.Snapshot)] = "None"
                }
            };
            yield return new ToggleScope
            {
                ScopeFactories =
                {
                    () => new CorrelateScope(),
                    () => new MeasureScope(),
                    () => new BufferScope(),
                    () => new CacheScope(),
                }
            };
            yield return new SerializeProperty
            {
                Serialize = new SerializeToJson()
            };
            yield return new CapitalizeValue
            {
                PropertyNames =
                {
                    nameof(LoggableProperty.Environment),
                    nameof(LoggableProperty.Layer),
                    nameof(LoggableProperty.Category),
                    nameof(LoggableProperty.Unit),
                    nameof(SerializableProperty.Snapshot),
                }
            };
            yield return new RenameProperty
            {
                Mappings =
                {
                    [nameof(LogLevel)] = "Level"
                }
            };
            yield return new Debug();
            yield return new Echo();
        }
    }

    /// <summary>
    /// Gets a pipeline that contains only popular features like timestamp and message.
    /// </summary>
    public static IEnumerable<ILoggerNode> Popular
    {
        get
        {
            yield return new InvokePropertyService
            {
                Services = { new Timestamp<DateTimeUtc>() }
            };
            yield return new InvokeEntryAction();
            yield return new GuessProperties();
            yield return new FilterEntries();
            yield return new Echo();
        }
    }

    /// <summary>
    /// Gets a pipeline that does not contain any features but property creation and echo.
    /// </summary>
    public static IEnumerable<ILoggerNode> Minimal
    {
        get
        {
            yield return new InvokeEntryAction();
            yield return new GuessProperties();
            yield return new Echo();
        }
    }
}