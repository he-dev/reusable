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
        public static IEnumerable<ILoggerNode> Default
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
                        [nameof(ExecutionLayers.Service)] = LogLevel.Debug,
                        [nameof(ExecutionLayers.IO)] = LogLevel.Trace,
                        [nameof(ExecutionLayers.Database)] = LogLevel.Trace,
                        [nameof(ExecutionLayers.Network)] = LogLevel.Trace,
                        [nameof(ExecutionLayers.Telemetry)] = LogLevel.Information,
                        [nameof(ExecutionLayers.Business)] = LogLevel.Information,
                        [nameof(ExecutionLayers.Presentation)] = LogLevel.Debug,
                    }
                };
                yield return new Fallback
                {
                    Properties =
                    {
                        [Names.Properties.Level] = LogLevel.Information,
                        [Names.Properties.Layer] = "Undefined",
                        [Names.Properties.Category] = "Undefined"
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
                        //new CollectFlowTelemetry(),
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
        
        public static IEnumerable<ILoggerNode> Minimal
        {
            get
            {
                yield return new Echo();
            }
        }
    }
}