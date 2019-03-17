using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Reflection;

namespace Reusable
{
    public static class Graph
    {
        public static IEnumerable<(T U, T V)> ToDirectedGraph<T>(this IEnumerable<KeyValuePair<T, IEnumerable<T>>> source)
        {
            // Convert dictionary into directed graph.
            return source.SelectMany(x => x.Value.Select(y => (x.Key, y)));
        }

        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<(T U, T V)> source, IEqualityComparer<T> comparer)
        {
            var edges = new HashSet<(T u, T v)>(source);

            // First, find a list of "start nodes" which have no incoming edges;
            // at least one such node must exist in a non-empty acyclic graph.
            var startNodes = edges.Select(e => e.u).Where(u => edges.Select(e => e.v).All(v => !comparer.Equals(u, v)));
            var topLevelNodes = new Stack<T>(startNodes);

            while (topLevelNodes.Any())
            {
                var node = topLevelNodes.Pop();
                yield return node;

                var outgoingEdges = edges.Where(e => comparer.Equals(e.u, node)).ToList();
                foreach (var edge in outgoingEdges)
                {
                    edges.Remove(edge);

                    var hasIncomingEdges = edges.Any(e => comparer.Equals(e.v, edge.v));
                    if (!hasIncomingEdges)
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
}
