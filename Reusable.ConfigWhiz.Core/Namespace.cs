using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.ConfigWhiz
{
    internal static class Namespace
    {
        public static IEnumerable<IEnumerable<string>> ExplodeNamespaces(this IEnumerable<string> namespaces)
        {
            return namespaces.Select(Split('.'));

            Func<string, IEnumerable<string>> Split(params char[] separators) => s => s.Split(separators);
        }

        public static IEnumerable<string> Split(string @namespace, params char[] separators) => @namespace.Split(separators);

        public static (IEnumerable<string> Common, IEnumerable<IEnumerable<string>> Distinct) ReduceNamespaces(this IEnumerable<IEnumerable<string>> namespaces)
        {
            var common = new List<string>();
            while (FirstEquals(namespaces, out string name))
            {
                common.Add(name);
                namespaces = namespaces.Select(SkipFirst());
            }

            return (common, namespaces);
        }

        public static IEnumerable<string> CommonNamespace(this IEnumerable<IEnumerable<string>> namespaces)
        {
            while (FirstEquals(namespaces, out string first))
            {
                yield return first;
                namespaces = namespaces.Select(SkipFirst());
            }
        }

        private static bool FirstEquals<T>(IEnumerable<IEnumerable<T>> values, out T first)
        {
            var distinct = values.Select(ns => ns.FirstOrDefault()).Distinct().ToList();
            switch (distinct.Count)
            {
                case 1:
                    first = distinct.Single();
                    return true;
                default:
                    first = default(T);
                    return false;
            }
        }

        private static Func<IEnumerable<string>, IEnumerable<string>> SkipFirst() => values => values.Skip(1);
    }
}