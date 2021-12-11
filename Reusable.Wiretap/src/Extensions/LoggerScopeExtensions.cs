using System;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Nodes.Scopeable;

namespace Reusable.Wiretap.Extensions
{
    public static class LoggerScopeExtensions
    {
        public static ILogScope WithCorrelationId(this ILogScope scope, object correlationId)
        {
            return scope.Also(x => x.First.Node<CorrelateScope>().CorrelationId = correlationId);
        }
        
        public static ILogScope WithCorrelationHandle(this ILogScope scope, object correlationHandle)
        {
            return scope.Also(x => x.First.Node<CorrelateScope>().CorrelationHandle = correlationHandle);
        }

        public static ILogScope UseBuffer(this ILogScope scope)
        {
            // Branch-node is properly initialized at this point.
            return scope.Also(x => x.First.Node<BufferScope>().Enable());
        }

        /// <summary>
        /// Activates a new MemoryNode.
        /// </summary>
        public static ILogScope UseInMemoryCache(this ILogScope scope)
        {
            return scope.Also(x => x.First.Node<CacheScope>().Enable());
        }

        /// <summary>
        /// Activates a new stopwatch and returns it.
        /// </summary>
        [Obsolete("Don't use this API. It's been replaced by the BranchNode. It's empty and provided for backward compatibility.")]
        public static ILogScope UseStopwatch(this ILogScope scope)
        {
            //return scope.Append(new StopwatchNode());
            return scope;
        }
        
        
        public static CorrelateScope Correlation(this ILogScope scope) => scope.First.Node<CorrelateScope>();
        
        public static BufferScope Buffer(this ILogScope scope) => scope.First.Node<BufferScope>();
        
        /// <summary>
        /// Gets the MemoryNode in current scope.
        /// </summary>
        public static CacheScope InMemoryCache(this ILogScope scope) => scope.First.Node<CacheScope>();
        
        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static MeasureScope Stopwatch(this ILogScope scope) => scope.First.Node<MeasureScope>();
        
        //public static CollectFlowTelemetry Flow(this IFlowScope flowScope) => flowScope.First.Node<CollectFlowTelemetry>();
    }
}