using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Exceptionize;

namespace Reusable.Wiretap.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object?> ToDictionary(this IEnumerable<(string Name, object? Value)> source)
        {
            return source.ToDictionary(x => x.Name, x => x.Value);
        }

        public static IEnumerable<(string Name, object? Value)> EnumerateProperties(this object obj)
        {
            return obj switch
            {
                IDictionary<string, object?> dictionary => dictionary.Select(x => (x.Key, x.Value)),
                _ =>
                    from p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    select (p.Name, p.GetValue(obj))
            };
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