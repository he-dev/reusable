using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LoggerNodeHelpers
    {
        public static LoggerFactory UseBuffer(this LoggerFactory loggerFactory, Action<BufferNode>? configure = default) => loggerFactory.Use(configure);
        public static LoggerFactory UseBuilder(this LoggerFactory loggerFactory, Action<BuilderNode>? configure = default) => loggerFactory.Use(configure);
        //public static LoggerFactory UseConstant(this LoggerFactory loggerFactory, Action<ConstantNode> configure) => loggerFactory.Use(configure);
        //public static LoggerFactory UseConstant(this LoggerFactory loggerFactory, params (string Name, object Value)[] constants) => loggerFactory.Use<ConstantNode>(n => n.Values.AddRangeSafely(constants));
        //public static LoggerFactory UseCorrelation(this LoggerFactory loggerFactory, Action<CorrelationNode> configure = default) => loggerFactory.Use(configure);
        public static LoggerFactory UseScope(this LoggerFactory loggerFactory, Action<ScopeNode>? configure = default) => loggerFactory.Use(configure);
        public static LoggerFactory UseEcho(this LoggerFactory loggerFactory, Action<EchoNode> configure) => loggerFactory.Use(configure);
        public static LoggerFactory UseEcho(this LoggerFactory loggerFactory, params ILogRx[] rx) => loggerFactory.Use<EchoNode>(n => n.Rx.AddRange(rx));
        public static LoggerFactory UseFallback(this LoggerFactory loggerFactory, Action<FallbackNode> configure) => loggerFactory.Use(configure);
        public static LoggerFactory UseFallback(this LoggerFactory loggerFactory, params (SoftString Name, object Value)[] defaults) => loggerFactory.Use<FallbackNode>(n => n.Defaults.AddRangeSafely(defaults));
        public static LoggerFactory UseFilter(this LoggerFactory loggerFactory, Func<LogEntry, bool> filter) => loggerFactory.Use(new FilterNode(filter));
        public static LoggerFactory UseDelegate(this LoggerFactory loggerFactory, Action<DelegateNode>? configure = default) => loggerFactory.Use(configure);
        public static LoggerFactory UseObjectMapper(this LoggerFactory loggerFactory, Action<ObjectMapperNode> configure) => loggerFactory.Use(configure);
        public static LoggerFactory UseObjectMapper(this LoggerFactory loggerFactory, params ObjectMapperNode.Mapping[] mappings) => loggerFactory.Use<ObjectMapperNode>(n => n.Mappings.AddRange(mappings));
        public static LoggerFactory UseDestructure(this LoggerFactory loggerFactory, Action<DestructureNode>? configure = default) => loggerFactory.Use(configure);
        public static LoggerFactory UsePropertyMapper(this LoggerFactory loggerFactory, Action<PropertyMapperNode> configure) => loggerFactory.Use(configure);
        public static LoggerFactory UsePropertyMapper(this LoggerFactory loggerFactory, params (SoftString From, string To)[] mappings) => loggerFactory.Use<PropertyMapperNode>(n => n.Mappings.AddRangeSafely(mappings));
        public static LoggerFactory UseService(this LoggerFactory loggerFactory, Action<ServiceNode> configure) => loggerFactory.Use(configure);
        public static LoggerFactory UseService(this LoggerFactory loggerFactory, params IService[] scalars) => loggerFactory.Use<ServiceNode>(n => n.Services.AddRange(scalars));
        public static LoggerFactory UseSerializer(this LoggerFactory loggerFactory, Action<SerializerNode>? configure = default) => loggerFactory.Use(configure);
        public static LoggerFactory UseStopwatch(this LoggerFactory loggerFactory, Action<StopwatchNode>? configure = default) => loggerFactory.Use(configure);
    }
}