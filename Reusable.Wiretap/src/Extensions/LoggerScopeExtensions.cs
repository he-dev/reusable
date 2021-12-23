using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes.Scopeable;

namespace Reusable.Wiretap.Extensions;

public static class LoggerScopeExtensions
{
    public static ILoggerScope WithCorrelationId(this ILoggerScope scope, object correlationId)
    {
        return scope.Also(x => x.First.Node<CorrelateScope>().CorrelationId = correlationId);
    }
        
    public static ILoggerScope WithCorrelationHandle(this ILoggerScope scope, object correlationHandle)
    {
        return scope.Also(x => x.First.Node<CorrelateScope>().CorrelationHandle = correlationHandle);
    }

    public static ILoggerScope UseBuffer(this ILoggerScope scope)
    {
        // Branch-node is properly initialized at this point.
        return scope.Also(x => x.First.Node<BufferScope>().Enable());
    }

    /// <summary>
    /// Activates a new MemoryNode.
    /// </summary>
    public static ILoggerScope UseInMemoryCache(this ILoggerScope scope)
    {
        return scope.Also(x => x.First.Node<CacheScope>().Enable());
    }
    
    public static ILoggerScope WorkItem(this ILoggerScope scope, object value)
    {
        return scope.Also(x => x.Items[nameof(WorkItem)] = value);
    }

    public static CorrelateScope Correlation(this ILoggerScope scope) => scope.First.Node<CorrelateScope>();
        
    public static BufferScope Buffer(this ILoggerScope scope) => scope.First.Node<BufferScope>();
        
    /// <summary>
    /// Gets the MemoryNode in current scope.
    /// </summary>
    public static CacheScope InMemoryCache(this ILoggerScope scope) => scope.First.Node<CacheScope>();
        
    /// <summary>
    /// Gets the stopwatch in current scope.
    /// </summary>
    public static MeasureScope Stopwatch(this ILoggerScope scope) => scope.First.Node<MeasureScope>();
        
    //public static CollectFlowTelemetry Flow(this IFlowScope flowScope) => flowScope.First.Node<CollectFlowTelemetry>();
}