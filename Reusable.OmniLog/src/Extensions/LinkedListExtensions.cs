using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.OmniLog.v2
{
    public static class LinkedListExtensions
    {
        public static T InsertRelative<T>(this T middleware, T insert, IDictionary<Type, int> order) where T : ILinkedListNode<T>
        {
            if (middleware.Previous == null && middleware.Next == null)
            {
                throw new ArgumentException("There need to be at least two middleware.");
            }

            var first = middleware.First();
            var zip = first.Enumerate(m => m.Next).Zip(first.Enumerate(m => m.Next).Skip(1), (current, next) => (current, next));

            foreach (var (current, next) in zip)
            {
                var canInsert =
                    order[insert.GetType()] >= order[current.GetType()] &&
                    order[insert.GetType()] <= order[next.GetType()];
                if (canInsert)
                {
                    return current.InsertNext(insert);
                }
            }

            return default; // This should not never be reached.
        }

        public static T First<T>(this T middleware) where T : ILinkedListNode<T>
        {
            return middleware.Enumerate(m => m.Previous).Last();
        }

        public static T Last<T>(this T middleware) where T : ILinkedListNode<T>
        {
            return middleware.Enumerate(m => m.Next).Last();
        }

        public static IEnumerable<T> Enumerate<T>(this T middleware, Func<T, T> direction) where T : ILinkedListNode<T>
        {
            do
            {
                yield return middleware;
            }
            while ((middleware = direction(middleware)) != null);
        }
    }
}