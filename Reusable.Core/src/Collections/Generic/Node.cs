using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Collections.Generic
{
    public interface INode<T> where T : INode<T>
    {
        T Prev { get; set; }

        T Next { get; set; }
    }

    public static class NodeExtensions
    {
        public static T Chain<T>(this IEnumerable<T> source) where T : INode<T>
        {
            return source.Aggregate((current, next) => current.Append(next));
        }

        public static T Append<T>(this T a, T b) where T : INode<T>
        {
            var c = a.Next;

            b.Prev = a;
            b.Next = c;

            a.Next = b;
            if (c is {}) c.Prev = b;

            return b;
        }

        public static T Prepend<T>(this T c, T b) where T : INode<T>
        {
            var a = c.Prev;

            b.Prev = a;
            b.Next = c;

            if (a is {}) a.Next = b;
            c.Prev = b;

            return b;
        }

        public static T Head<T>(this T node) where T : INode<T> => node.EnumeratePrev().Last();

        public static T Tail<T>(this T node) where T : INode<T> => node.EnumerateNext().Last();

        public static IEnumerable<T> EnumerateNext<T>(this T n, bool includeSelf = true) where T : INode<T> => n.Enumerate(x => x.Next, includeSelf);

        public static IEnumerable<T> EnumeratePrev<T>(this T n, bool includeSelf = true) where T : INode<T> => n.Enumerate(x => x.Prev, includeSelf);

        private static IEnumerable<T> Enumerate<T>(this T n, Func<T, T> direction, bool includeSelf = true) where T : INode<T>
        {
            n = includeSelf ? n : direction(n);
            while (n is {})
            {
                yield return n;
                n = direction(n);
            }
        }
    }
}