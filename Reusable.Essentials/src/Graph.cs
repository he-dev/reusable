using System.Collections.Generic;
using System.Linq;

namespace Reusable.Essentials;

public static class Graph
{
    public static IEnumerable<(T U, T V)> ToDirectedGraph<T>(this IEnumerable<(T Node, IEnumerable<T> OutgoingNodes)> source)
    {
        // Convert dictionary into directed graph.
        return 
            from item in source
            from outgoingNode in item.OutgoingNodes
            select (item.Node, outgoingNode);
    }

    public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<(T U, T V)> source, IEqualityComparer<T> comparer)
    {
        var edges = new HashSet<(T u, T v)>(source);	

        // Find a list of "start nodes". These nodes have no incoming edges;
        // at least one such node must exist in a non-empty acyclic graph.
            
        var startNodes =
            from x in edges
            let incomingNodes =
                from y in edges
                where comparer.Equals(x.u, y.v)
                select y
            where !incomingNodes.Any()
            select x.u;
			
        var topLevelNodes = new Stack<T>(startNodes.Distinct());

        while (topLevelNodes.Any())
        {
            var node = topLevelNodes.Pop();
            yield return node;

            var outgoingEdges = edges.Where(e => comparer.Equals(e.u, node)).ToList();
            foreach (var edge in outgoingEdges)
            {
                edges.Remove(edge);
				
                var incomingEdges = edges.Where(e => comparer.Equals(e.v, edge.v));
                if (!incomingEdges.Any())
                {
                    topLevelNodes.Push(edge.v);
                }
            }			
        }

        if (edges.Any())
        {
            throw DynamicException.Create("GraphCycle", $"Detected one or more cycles between the following nodes: [{string.Join(", ", edges)}]");
        }
    }
}