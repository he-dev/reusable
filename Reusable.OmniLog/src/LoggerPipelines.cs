using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Properties;
using Reusable.OmniLog.Services;
using Buffer = Reusable.OmniLog.Nodes.Buffer;

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
                yield return new InjectAnonymousDelegate();
                yield return new CreateProperty();
                //yield return new BuilderNode();
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
                yield return new Branch
                {
                    CreateNodes = () => new ILoggerNode[]
                    {
                        new Correlate(),
                        new MeasureElapsedTime(),
                        new Buffer(),
                        new CacheInMemory(),
                        new CollectWorkItemTelemetry(),
                    }
                };
                yield return new SerializeProperty
                {
                    Serialize = new SerializeToJson()
                };
                yield return new FormatAsCamelCase
                {
                    PropertyNames =
                    {
                        Names.Properties.Logger,
                        Names.Properties.Layer,
                        Names.Properties.Category,
                        Names.Properties.SnapshotName,
                    }
                };
                yield return new RenameProperty();
                yield return new Echo();
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

        public static ILoggerNodeConfiguration<T> Configure<T>(this IEnumerable<ILoggerNode> nodes) where T : ILoggerNode
        {
            return new LoggerNodeConfiguration<T>(nodes);
        }

        public static ILoggerFactory ToLoggerFactory(this IEnumerable<ILoggerNode> nodes) => new LoggerFactory(nodes);
    }


    // ReSharper disable once UnusedTypeParameter - This interface carries the T so it must not be removed.
    public interface ILoggerNodeConfiguration<T> : IEnumerable<ILoggerNode> { }

    internal class LoggerNodeConfiguration<T> : ILoggerNodeConfiguration<T> where T : ILoggerNode
    {
        private readonly IEnumerable<ILoggerNode> _nodes;

        public LoggerNodeConfiguration(IEnumerable<ILoggerNode> nodes) => _nodes = nodes;

        public IEnumerator<ILoggerNode> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nodes).GetEnumerator();
    }

    public static class LoggerNodeConfigurationExtensions
    {
        public static ILoggerNodeConfiguration<T> Add<T, TValue>
        (
            this ILoggerNodeConfiguration<T> configuration,
            Func<T, ICollection<TValue>> selector,
            params TValue[] values
        ) where T : ILoggerNode
        {
            return new LoggerNodeConfiguration<T>(configuration.Configure<T>(node =>
            {
                foreach (var value in values)
                {
                    selector(node).Add(value);
                }
            }));
        }

        public static ILoggerNodeConfiguration<T> Add<T, TKey, TValue>
        (
            this ILoggerNodeConfiguration<T> configuration,
            Func<T, IDictionary<TKey, TValue>> selector,
            params (TKey Key, TValue Value)[] items
        ) where T : ILoggerNode
        {
            return new LoggerNodeConfiguration<T>(configuration.Configure<T>(node =>
            {
                foreach (var (key, value) in items)
                {
                    selector(node).Add(key, value);
                }
            }));
        }
    }
}