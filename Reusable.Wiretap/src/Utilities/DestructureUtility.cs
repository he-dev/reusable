using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Exceptionize;

namespace Reusable.Wiretap.Utilities
{
    public static class DestructureUtility
    {
        public static IDictionary<string, object> ToDictionary<T>(this T obj)
        {
            return obj switch
            {
                IDictionary<string, object> dictionary => dictionary,
                {} => obj.EnumerateProperties().ToDictionary(x => x.Name, x => x.Value),
                null => new Dictionary<string, object>()
            };
        }

        private static IEnumerable<(string Name, object Value)> EnumerateProperties(this object obj)
        {
            return
                from p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                select (p.Name, p.GetValue(obj));
        }

        private static Type ValidateIsAnonymous(this Type type)
        {
            var isAnonymous = type.Name.StartsWith("<>f__AnonymousType");

            return
                isAnonymous
                    ? type
                    : throw DynamicException.Create("Snapshot", "Snapshot must be either an anonymous type or a dictionary");
        }
    }
}