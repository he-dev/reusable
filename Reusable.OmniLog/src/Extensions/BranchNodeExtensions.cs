using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class BranchNodeExtensions
    {
        public static Correlate Correlation(this IBranch branch) => branch.First.Node<Correlate>();
        
        public static Buffer Buffer(this IBranch branch) => branch.First.Node<Buffer>();
        
        /// <summary>
        /// Gets the MemoryNode in current scope.
        /// </summary>
        public static CacheInMemory InMemoryCache(this IBranch branch) => branch.First.Node<CacheInMemory>();
        
        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static MeasureElapsedTime Stopwatch(this IBranch branch) => branch.First.Node<MeasureElapsedTime>();
        
        public static CollectWorkItemTelemetry WorkItem(this IBranch branch) => branch.First.Node<CollectWorkItemTelemetry>();
    }
}