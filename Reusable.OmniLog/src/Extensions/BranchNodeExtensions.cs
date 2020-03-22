using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class BranchNodeExtensions
    {
        public static CorrelationNode Correlation(this BranchNode logger) => logger.First.Node<CorrelationNode>();
        
        public static BufferNode Buffer(this BranchNode logger) => logger.First.Node<BufferNode>();
        
        /// <summary>
        /// Gets the MemoryNode in current scope.
        /// </summary>
        public static MemoryNode Memory(this BranchNode logger) => logger.First.Node<MemoryNode>();
        
        /// <summary>
        /// Gets the stopwatch in current scope.
        /// </summary>
        public static StopwatchNode Stopwatch(this BranchNode logger) => logger.First.Node<StopwatchNode>();
        
        public static WorkItemNode WorkItem(this BranchNode branch) => branch.First.Node<WorkItemNode>();
    }
}