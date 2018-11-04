using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable
{
    public static class DependencyValidator
    {
        //public static void ValidateDependencies(Dictionary<string, IEnumerable<string>> dict)
        //{
        //    // Convert dictionary into directed graph.
        //    var edges = new HashSet<Tuple<string, string>>(dict.SelectMany(x => x.Value.Select(y => Tuple.Create(x.Key, y))));
        //    var sorted = new List<string>();

        //    //First, find a list of "start nodes" which have no incoming edges and insert them into a set S;
        //    // at least one such node must exist in a non-empty acyclic graph.
        //    var startNodes = dict.Keys.Where(n => edges.All(e => !e.Item2.Equals(n)));
        //    var topLevelNodes = new Stack<string>(startNodes);

        //    while (topLevelNodes.Any())
        //    {
        //        var node = topLevelNodes.Pop();
        //        sorted.Add(node);
        //        var outgoingEdges = edges.Where(e => e.Item1.Equals(node)).ToList();
        //        foreach (var edge in outgoingEdges)
        //        {
        //            edges.Remove(edge);
        //            var childNode = edge.Item2;
        //            var childHasIncomingEdges = edges.Any(e => e.Item2.Equals(childNode));
        //            if (!childHasIncomingEdges)
        //            {
        //                topLevelNodes.Push(childNode);
        //            }
        //        }
        //    }

        //    if (edges.Any())
        //    {
        //        var circularNamesFormatted = edges.Select(e => e.Item1).Distinct().Reverse().QuoteWith("'").Join(", ").EncloseWith('[', ']', 1);
        //        throw DynamicException.Factory.CreateDynamicException(new CircularDependencyExceptionTemplate { Names = circularNamesFormatted });
        //    }

        //    var missingNodes = sorted.Where(node => !dict.ContainsKey(node)).ToList();
        //    if (missingNodes.Any())
        //    {
        //        var missingNamesFormatted = missingNodes.QuoteWith("'").Join(", ").EncloseWith('[', ']', 1);
        //        throw DynamicException.Factory.CreateDynamicException(new MissingDependencyExceptionTemplate {Names = missingNamesFormatted});
        //    }
        //}

        [Obsolete]
        public static void ValidateDependencies<TKey>(Dictionary<TKey, IEnumerable<TKey>> dependencies)
        {
            // Convert dictionary into directed graph.
            var edges = new HashSet<Tuple<TKey, TKey>>(dependencies.SelectMany(x => x.Value.Select(y => Tuple.Create(x.Key, y))));
            var sorted = new List<TKey>();

            //First, find a list of "start nodes" which have no incoming edges and insert them into a set S;
            // at least one such node must exist in a non-empty acyclic graph.
            var startNodes = dependencies.Keys.Where(n => edges.All(e => !e.Item2.Equals(n)));
            var topLevelNodes = new Stack<TKey>(startNodes);

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
                var circularNamesFormatted = edges.Select(e => e.Item1).Distinct().Reverse().QuoteAllWith("'").Join(", ").EncloseWith('[', ']', 1);
                throw DynamicException.Factory.CreateDynamicException(new CircularDependencyExceptionTemplate { Names = circularNamesFormatted });
            }

            var missingNodes = sorted.Where(node => !dependencies.ContainsKey(node)).ToList();
            if (missingNodes.Any())
            {
                var missingNamesFormatted = missingNodes.QuoteAllWith("'").Join(", ").EncloseWith('[', ']', 1);
                throw DynamicException.Factory.CreateDynamicException(new MissingDependencyExceptionTemplate { Names = missingNamesFormatted });
            }
        }

        public class CircularDependencyExceptionTemplate : DynamicExceptionTemplate
        {
            public override string Message => $"Circular dependencies are not allowed: {Names}.";
            public string Names { get; set; }
        }

        public class MissingDependencyExceptionTemplate : DynamicExceptionTemplate
        {
            public override string Message => $"Some dependencies are missing: {Names}.";
            public string Names { get; set; }
        }        
    }

    //public class CircularDependencyException : Exception
    //{
    //    public CircularDependencyException(IEnumerable<string> path)
    //        : base($"Circular dependecies: {path.Reverse().QuoteWith('\'').Join(", ").EncloseWith('[', ']', 1)}")
    //    { }
    //}

    //public class MissingDependencyException : Exception
    //{
    //    public MissingDependencyException(IEnumerable<string> names)
    //        : base($"Missing dependecies: {names.QuoteWith('\'').Join(", ").EncloseWith('[', ']', 1)}")
    //    { }
    //}
}
