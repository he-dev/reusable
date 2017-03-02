using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public static class Validator
    {
        public static void ValidateDependencies(Dictionary<string, IEnumerable<string>> dict)
        {
            // Convert dictionary into directed graph.
            var edges = new HashSet<Tuple<string, string>>(dict.SelectMany(x => x.Value.Select(y => Tuple.Create(x.Key, y))));
            var sorted = new List<string>();

            //First, find a list of "start nodes" which have no incoming edges and insert them into a set S;
            // at least one such node must exist in a non-empty acyclic graph.
            var startNodes = dict.Keys.Where(n => edges.All(e => !e.Item2.Equals(n)));
            var topLevelNodes = new Stack<string>(startNodes);

            while (topLevelNodes.Any())
            {
                var node = topLevelNodes.Pop();
                sorted.Add(node);
                var outgoingEdges = edges.Where(e => e.Item1.Equals(node)).ToList();
                foreach (var edge in outgoingEdges)
                {
                    edges.Remove(edge);
                    var childNode = edge.Item2;
                    var childHasIncomingEdges = edges.Any(e => e.Item2.Equals(childNode));
                    if (!childHasIncomingEdges)
                    {
                        topLevelNodes.Push(childNode);
                    }
                }
            }

            if (edges.Any())
            {
                throw new CircularDependencyException(edges.Select(e => e.Item1).Distinct());
            }

            var missingNodes = sorted.Where(node => !dict.ContainsKey(node)).ToList();
            if (missingNodes.Any())
            {
                throw new MissingDependencyException(missingNodes);
            }
        }
    }

    public class CircularDependencyException : Exception
    {
        public CircularDependencyException(IEnumerable<string> path)
            : base($"Circular dependecies: [{string.Join(", ", path.Reverse().Select(x => $"'{x}'"))}]")
        { }
    }

    public class MissingDependencyException : Exception
    {
        public MissingDependencyException(IEnumerable<string> names)
            : base($"Missing dependecies: [{string.Join(", ", names.Select(x => $"'{x}'"))}]")
        { }
    }
}
