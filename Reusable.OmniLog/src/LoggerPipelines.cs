using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Properties;

namespace Reusable.OmniLog
{
    public static class LoggerPipelines
    {
        public static IEnumerable<ILoggerNode> Default
        {
            get
            {
                yield return new PropertyNode
                {
                    Properties = { new Timestamp<DateTimeUtc>() }
                };
                yield return new DelegateNode();
                yield return new PropertyFactoryNode();
                yield return new BuilderNode();
                yield return new DestructureNode();
                yield return new ObjectMapperNode();
                yield return new FilterNode();
                yield return new MapValueToLogLevelNode
                {
                    Property = Names.Default.Layer,
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
                yield return new FallbackNode
                {
                    Properties =
                    {
                        [Names.Default.Level] = LogLevel.Information,
                        [Names.Default.Layer] = "Undefined",
                        [Names.Default.Category] = "Undefined"
                    }
                };
                yield return new BranchNode
                {
                    CreateNodes = () => new ILoggerNode[]
                    {
                        new CorrelationNode(),
                        new StopwatchNode(),
                        new BufferNode(),
                        new MemoryNode(),
                        new WorkItemNode(),
                    }
                };
                yield return new SerializerNode();
                yield return new CamelCaseNode
                {
                    PropertyNames =
                    {
                        Names.Default.Layer,
                        Names.Default.Category,
                    }
                };
                yield return new PropertyMapperNode();
                yield return new EchoNode();
            }
        }

        public static IEnumerable<ILoggerNode> Configure<T>(this IEnumerable<ILoggerNode> nodes, Action<T> configure) where T : ILoggerNode
        {
            foreach (var node in nodes)
            {
                if (node is T configurable)
                {
                    configure(configurable);
                }
                yield return node;
            }
        }
        
        public static ILoggerFactory ToLoggerFactory(this IEnumerable<ILoggerNode> nodes) => new LoggerFactory(nodes);
    }
}