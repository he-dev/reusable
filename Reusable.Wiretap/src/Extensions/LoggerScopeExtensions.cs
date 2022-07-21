using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes.Scopeable;

namespace Reusable.Wiretap.Extensions;

public static class LoggerScopeExtensions
{
    public static ILoggerScope WithId(this ILoggerScope scope, object value)
    {
        return scope.Also(x => x.Correlation().Id = value);
    }
        
    public static ILoggerScope WithName(this ILoggerScope scope, object value)
    {
        return scope.Also(x => x.Correlation().Name = value);
    }

    public static ILoggerScope UseBuffer(this ILoggerScope scope)
    {
        // Branch-node is properly initialized at this point.
        return scope.Also(x => x.Items.Add(nameof(ScopeBuffer), true));
    }

    // /// <summary>
    // /// Activates a new MemoryNode.
    // /// </summary>
    // public static ILoggerScope UseInMemoryCache(this ILoggerScope scope)
    // {
    //     return scope.Also(x => x.Node<CacheScope>().Enable());
    // }
    
    public static ILoggerScope WorkItem(this ILoggerScope scope, object value)
    {
        return scope.Also(x => x.Items[nameof(WorkItem)] = value);
    }

    public static Correlation Correlation(this ILoggerScope scope) => scope.Node<Correlation>();
        
    public static ScopeBuffer Buffer(this ILoggerScope scope) => scope.Node<ScopeBuffer>();
        
    /// <summary>
    /// Gets the MemoryNode in current scope.
    /// </summary>
    public static CacheScope InMemoryCache(this ILoggerScope scope) => scope.Node<CacheScope>();
        
    /// <summary>
    /// Gets the stopwatch in current scope.
    /// </summary>
    public static ScopeElapsed Stopwatch(this ILoggerScope scope) => scope.Node<ScopeElapsed>();
        
    //public static CollectFlowTelemetry Flow(this IFlowScope flowScope) => flowScope.First.Node<CollectFlowTelemetry>();
}