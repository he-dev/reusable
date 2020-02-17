using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Collections.Generic
{
    [PublicAPI]
    public static class NodeExtensions
    {
        [DebuggerStepThrough]
        public static T Chain<T>(this IEnumerable<T> source) where T : class, INode<T> => source.Aggregate(Append);

        public static T Append<T>(this T a, T b) where T : class, INode<T>
        {
            var c = a.Next;

            b.Prev = a;
            b.Next = c;

            a.Next = b;
            if (c is {}) c.Prev = b;

            return b;
        }

        public static T Prepend<T>(this T c, T b) where T : class, INode<T>
        {
            var a = c.Prev;

            b.Prev = a;
            b.Next = c;

            if (a is {}) a.Next = b;
            c.Prev = b;

            return b;
        }

        [DebuggerStepThrough]
        public static T Head<T>(this T node) where T : class, INode<T> => node.EnumeratePrev().Last();

        [DebuggerStepThrough]
        public static T Tail<T>(this T node) where T : class, INode<T> => node.EnumerateNext().Last();

        [DebuggerStepThrough]
        public static IEnumerable<T> EnumerateNext<T>(this T n, bool includeSelf = true) where T : class, INode<T> => n.Enumerate(x => x.Next, includeSelf);

        [DebuggerStepThrough]
        public static IEnumerable<T> EnumeratePrev<T>(this T n, bool includeSelf = true) where T : class, INode<T> => n.Enumerate(x => x.Prev, includeSelf);

        private static IEnumerable<T> Enumerate<T>(this T n, Func<T, T?> direction, bool includeSelf = true) where T : class, INode<T>
        {
            var c = includeSelf ? n : direction(n);
            while (c is {})
            {
                yield return n;
                c = direction(c);
            }
        }
    }
}