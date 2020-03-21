using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LoggerNodeHelpers
    {
        //public static LoggerFactory UseConstant(this LoggerFactory loggerFactory, Action<ConstantNode> configure) => loggerFactory.Use(configure);
        //public static LoggerFactory UseConstant(this LoggerFactory loggerFactory, params (string Name, object Value)[] constants) => loggerFactory.Use<ConstantNode>(n => n.Values.AddRangeSafely(constants));
        //public static LoggerFactory UseCorrelation(this LoggerFactory loggerFactory, Action<CorrelationNode> configure = default) => loggerFactory.Use(configure);
        public static LoggerFactoryBuilder UseBuffer(this LoggerFactoryBuilder builder, Action<BufferNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UseBuilder(this LoggerFactoryBuilder builder, Action<BuilderNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UseCamelCase(this LoggerFactoryBuilder builder, Action<CamelCaseNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UseScope(this LoggerFactoryBuilder builder, Action<BranchNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UseEcho(this LoggerFactoryBuilder builder, Action<EchoNode> configure) => builder.Use(configure);
        public static LoggerFactoryBuilder UseEcho(this LoggerFactoryBuilder builder, params ILogRx[] rx) => builder.Use<EchoNode>(n => n.Rx.AddRange(rx));
        public static LoggerFactoryBuilder UseEcho(this LoggerFactoryBuilder builder, IEnumerable<ILogRx> rx) => builder.Use<EchoNode>(n => n.Rx.AddRange(rx));
        public static LoggerFactoryBuilder UseFallback(this LoggerFactoryBuilder builder, Action<FallbackNode> configure) => builder.Use(configure);
        public static LoggerFactoryBuilder UseFallback(this LoggerFactoryBuilder builder, params (string Name, object Value)[] defaults) => builder.Use<FallbackNode>(n => n.Defaults.AddRangeSafely(defaults));
        public static LoggerFactoryBuilder UseFilter(this LoggerFactoryBuilder builder, Func<ILogEntry, bool> filter) => builder.Use(new FilterNode(filter));
        public static LoggerFactoryBuilder UseDelegate(this LoggerFactoryBuilder builder, Action<DelegateNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UseObjectMapper(this LoggerFactoryBuilder builder, Action<ObjectMapperNode> configure) => builder.Use(configure);
        public static LoggerFactoryBuilder UseObjectMapper(this LoggerFactoryBuilder builder, params ObjectMapperNode.Mapping[] mappings) => builder.Use<ObjectMapperNode>(n => n.Mappings.AddRange(mappings));
        public static LoggerFactoryBuilder UseDestructure(this LoggerFactoryBuilder builder, Action<DestructureNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UsePropertyMapper(this LoggerFactoryBuilder builder, Action<PropertyMapperNode> configure) => builder.Use(configure);
        public static LoggerFactoryBuilder UsePropertyMapper(this LoggerFactoryBuilder builder, params (string From, string To)[] mappings) => builder.Use<PropertyMapperNode>(n => n.Mappings.AddRangeSafely(mappings));
        public static LoggerFactoryBuilder UseService(this LoggerFactoryBuilder builder, Action<ServiceNode> configure) => builder.Use(configure);
        public static LoggerFactoryBuilder UseService(this LoggerFactoryBuilder builder, params IService[] scalars) => builder.Use<ServiceNode>(n => n.Services.AddRange(scalars));
        public static LoggerFactoryBuilder UseSerializer(this LoggerFactoryBuilder builder, Action<SerializerNode>? configure = default) => builder.Use(configure);
        public static LoggerFactoryBuilder UseStopwatch(this LoggerFactoryBuilder builder, Action<StopwatchNode>? configure = default) => builder.Use(configure);

        public static T Enable<T>(this T node) where T : ILoggerNode
        {
            return node.Pipe(x => x.Enabled = true);
        }
        
        
    }
}