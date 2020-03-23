using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class FlowScopeExtensions
    {
        public static Correlate Correlation(this IFlowScope flowScope) => flowScope.First.Node<Correlate>();
        
        public static BufferLog Buffer(this IFlowScope flowScope) => flowScope.First.Node<BufferLog>();
        
        /// <summary>
        /// Gets the MemoryNode in current scope.
        /// </summary>
        public static CacheInMemory InMemoryCache(this IFlowScope flowScope) => flowScope.First.Node<CacheInMemory>();
        
        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static MeasureElapsedTime Stopwatch(this IFlowScope flowScope) => flowScope.First.Node<MeasureElapsedTime>();
        
        public static CollectFlowTelemetry Flow(this IFlowScope flowScope) => flowScope.First.Node<CollectFlowTelemetry>();
    }
}