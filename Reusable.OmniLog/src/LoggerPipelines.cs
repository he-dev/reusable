using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Properties;
using Reusable.OmniLog.Services;

namespace Reusable.OmniLog
{
    public static class LoggerPipelines
    {
        /// <summary>
        /// Get a pipeline that contains all supported features.
        /// </summary>
        public static IEnumerable<ILoggerNode> Complete
        {
            get
            {
                yield return new AttachProperty
                {
                    Properties = { new Timestamp<DateTimeUtc>() }
                };
                yield return new InjectAnonymousAction();
                yield return new CreateProperty();
                yield return new Destructure();
                yield return new MapObject();
                yield return new Filter();
                yield return new MapPropertyToLogLevel
                {
                    PropertyName = Names.Properties.Layer,
                    Mapper = new MapStringToLogLevel
                    {
                        [nameof(TelemetryLayers.Dependency)] = LogLevel.Trace,
                        [nameof(TelemetryLayers.Application)] = LogLevel.Trace,
                        [nameof(TelemetryLayers.Business)] = LogLevel.Information,
                        [nameof(TelemetryLayers.Presentation)] = LogLevel.Trace,
                    }
                };
                yield return new Fallback
                {
                    Properties =
                    {
                        [Names.Properties.Level] = LogLevel.Information,
                        [Names.Properties.Layer] = "Undefined",
                        [Names.Properties.Category] = "Undefined",
                        [Names.Properties.Unit] = "Undefined",
                        [Names.Properties.Snapshot] = "Undefined"
                    }
                };
                yield return new ToggleScope
                {
                    CreateNodes = () => new ILoggerNode[]
                    {
                        new Correlate(),
                        new MeasureElapsedTime(),
                        new BufferLog(),
                        new CacheInMemory(),
                    }
                };
                yield return new SerializeProperty
                {
                    Serialize = new SerializeToJson()
                };
                yield return new FormatAsCamelCase
                {
                    Identifiers =
                    {
                        Names.Properties.Environment,
                        Names.Properties.Logger,
                        Names.Properties.Layer,
                        Names.Properties.Category,
                        Names.Properties.Unit,
                    }
                };
                yield return new RenameProperty();
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
                yield return new AttachProperty
                {
                    Properties = { new Timestamp<DateTimeUtc>() }
                };
                yield return new InjectAnonymousAction();
                yield return new CreateProperty();
                yield return new Filter();
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
                yield return new InjectAnonymousAction();
                yield return new CreateProperty();
                yield return new Echo();
            }
        }
    }
}