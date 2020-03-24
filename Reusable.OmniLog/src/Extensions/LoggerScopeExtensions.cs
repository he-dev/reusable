using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class LoggerScopeExtensions
    {
        public static ILoggerScope WithCorrelationId(this ILoggerScope scope, object correlationId)
        {
            return scope.Pipe(x => x.First.Node<Correlate>().CorrelationId = correlationId);
        }
        
        public static ILoggerScope WithCorrelationHandle(this ILoggerScope scope, object correlationHandle)
        {
            return scope.Pipe(x => x.First.Node<Correlate>().CorrelationHandle = correlationHandle);
        }

        public static ILoggerScope UseBuffer(this ILoggerScope scope)
        {
            // Branch-node is properly initialized at this point.
            return scope.Pipe(x => x.First.Node<BufferLog>().Enable());
        }

        /// <summary>
        /// Activates a new MemoryNode.
        /// </summary>
        public static ILoggerScope UseInMemoryCache(this ILoggerScope scope)
        {
            return scope.Pipe(x => x.First.Node<CacheInMemory>().Enable());
        }

        /// <summary>
        /// Activates a new stopwatch and returns it.
        /// </summary>
        [Obsolete("Don't use this API. It's been replaced by the BranchNode. It's empty and provided for backward compatibility.")]
        public static ILoggerScope UseStopwatch(this ILoggerScope scope)
        {
            //return scope.Append(new StopwatchNode());
            return scope;
        }
        
        
        public static Correlate Correlation(this ILoggerScope scope) => scope.First.Node<Correlate>();
        
        public static BufferLog Buffer(this ILoggerScope scope) => scope.First.Node<BufferLog>();
        
        /// <summary>
        /// Gets the MemoryNode in current scope.
        /// </summary>
        public static CacheInMemory InMemoryCache(this ILoggerScope scope) => scope.First.Node<CacheInMemory>();
        
        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static MeasureElapsedTime Stopwatch(this ILoggerScope scope) => scope.First.Node<MeasureElapsedTime>();
        
        //public static CollectFlowTelemetry Flow(this IFlowScope flowScope) => flowScope.First.Node<CollectFlowTelemetry>();
    }
}