using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reusable.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> NestedTypes(this Type type, Func<Type, bool> predicate = null)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (predicate == null || predicate(type))
            {
                yield return type;
            }

            var typeStack = new Stack<Type>(new[] { type });

            while (typeStack.Any())
            {
                type = typeStack.Pop();
                var nestedTypes = type.GetNestedTypes(BindingFlags.Public);
                foreach (var nestedType in nestedTypes.Where(t => predicate == null || predicate(t)))
                {
                    typeStack.Push(nestedType);
                    yield return nestedType;
                }
            }
        }

        public static bool IsStatic(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return type.IsAbstract && type.IsSealed;
        }

        public static bool Implements<T>(this Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (!type.IsInterface) { throw new ArgumentException($"Type {nameof(type)} must be an interface."); }

            return type.GetInterfaces().Contains(typeof(T));
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

        public static string ToShortString(this Type type)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            var typeReferenceExpression = new CodeTypeReferenceExpression(type);
            using (var writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, writer, new CodeGeneratorOptions());
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
