using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Reusable
{
    public static class Reflection
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

        public static string ToShortString(this MethodInfo method)
        {
            if (method == null) { throw new ArgumentNullException(nameof(method)); }

            var parameters = method.GetParameters().Select(p => $"{p.ParameterType.ToShortString()} {p.Name}");

            // public/internal/protected/private [static] [abstract/virtual/override] retVal

            var accessModifier = new[]
            {
                method.IsPublic ? "public" : string.Empty,
                method.IsAssembly ? "internal" : string.Empty,
                method.IsPrivate ? "private" : string.Empty,
                method.IsFamily ? "protected" : string.Empty,
            }
            .First(x => !string.IsNullOrEmpty(x));

            var inheritanceModifier = new[]
            {
                method.IsAbstract ? " abstract" : string.Empty,
                method.IsVirtual ? " virtual" : string.Empty,
                method.GetBaseDefinition() != method ? " override" : string.Empty,
            }
            .FirstOrDefault(x => !string.IsNullOrEmpty(x));

            var signature = new StringBuilder()
                .Append(method.DeclaringType?.FullName)
                .Append(" { ")
                .Append(accessModifier)
                .Append(method.IsStatic ? " static" : string.Empty)
                .Append(inheritanceModifier)
                .Append(method.GetCustomAttribute<AsyncStateMachineAttribute>() != null ? " async" : string.Empty)
                .Append(" ").Append(method.ReturnType.ToShortString())
                .Append(" ").Append(method.Name)
                .Append("(").Append(string.Join(", ", parameters)).Append(") { ... }")
                .Append(" } ")
                .ToString();

            return signature;
        }

        // --- non-extensions        

        public static Type GetElemenType(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            if (type.IsArray)
            {
                return type.GetElementType(); ;
            }

            if (type.IsList())
            {
                return type.GetGenericArguments()[0];
            }

            if (type.IsHashSet())
            {
                return type.GetGenericArguments()[0];
            }

            if (type.IsDictionary())
            {
                return type.GetGenericArguments()[1];
            }

            return null;
        }

        public static Type GetElemenType(object obj)
        {
            return GetElemenType(obj?.GetType());
        }

        public static Type GetElemenType<T>()
        {
            return GetElemenType(typeof(T));
        }
    }
}
