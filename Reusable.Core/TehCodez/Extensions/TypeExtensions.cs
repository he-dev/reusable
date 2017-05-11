using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reusable.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<(Type Type, int Depth)> Flatten(this Type type, Func<Type, bool> predicate = null)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            var typeStack = new Stack<(Type Type, int Depth)>();
            typeStack.Push((type, 0));

            while (typeStack.Any())
            {
                var current = typeStack.Pop();
                if (predicate == null || predicate(current.Type)) yield return current;

                foreach (var nestedType in current.Type.GetNestedTypes(BindingFlags.Public))
                {
                    typeStack.Push((nestedType, current.Depth + 1));
                }
            }
        }

        public static bool IsStatic(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return type.IsAbstract && type.IsSealed;
        }

        public static bool Implements(this Type type, Type checkType)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (!checkType.IsInterface) { throw new ArgumentException($"Type {nameof(type)} must be an interface."); }

            return type.GetInterfaces().Contains(checkType);
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            var isEnumerable =
                type
                    .GetInterfaces()
                    .Where(x => x.IsGenericType)
                    .Select(x => x.GetGenericTypeDefinition())
                    .Contains(typeof(IEnumerable<>));

            return isEnumerable && type != typeof(string);
        }

        public static bool IsEnumerable<T>(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            var isEnumerable =
                type
                    .GetInterfaces()
                    .Contains(typeof(IEnumerable<T>));

            return isEnumerable && type != typeof(string);
        }

        public static bool IsDictionary(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return
                type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static bool IsList(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return
                type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public static bool IsHashSet(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }
    }
}