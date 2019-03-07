using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Reflection
{
    public static class TypeExtensions
    {
        public static bool HasDefaultConstructor(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsStatic(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsAbstract && type.IsSealed;
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"Type {nameof(type)} must be an interface.");
            }

            return type.GetInterfaces().Contains(interfaceType);
        }

        public static bool IsEnumerableOfT(this Type type, params Type[] except)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return
                type.NotIn(except) &&
                type
                    .GetInterfaces()
                    .Append(type) // In case 'type' itself is an IEnumerable<>.
                    .Where(x => x.IsGenericType)
                    .Select(x => x.GetGenericTypeDefinition())
                    .Contains(typeof(IEnumerable<>));
        }

        public static bool IsEnumerableOf<T>(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type == typeof(string)) throw new ArgumentException(paramName: nameof(type), message: $"{nameof(type)} must not be a {typeof(string).Name}");

            return
                type
                    .GetInterfaces()
                    .Contains(typeof(IEnumerable<T>));
        }

        public static bool IsDictionary(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return
                type.IsGenericType &&
                type.GetGenericTypeDefinition().In(typeof(Dictionary<,>), typeof(IDictionary<,>));
        }

        public static bool IsList(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return
                type.IsGenericType &&
                type.GetGenericTypeDefinition().In(typeof(IList<>), typeof(List<>));
        }

        public static bool IsHashSet(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return
                type.IsGenericType &&
                type.GetGenericTypeDefinition().In(typeof(HashSet<>), typeof(ISet<>));
        }

        public static IEnumerable<(Type Type, int Depth)> SelectMany(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var typeStack = new Stack<(Type Type, int Depth)>();
            typeStack.Push((type, 0));

            while (typeStack.Any())
            {
                var current = typeStack.Pop();

                yield return current;

                foreach (var nestedType in current.Type.GetNestedTypes(BindingFlags.Public))
                {
                    typeStack.Push((nestedType, current.Depth + 1));
                }
            }
        }

        [NotNull]
        public static Type GetElementType([NotNull] Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsArray)
            {
                return type.GetElementType() ?? throw CreateArgumentException();
            }

            if (type.IsEnumerableOfT())
            {
                var genericArguments = type.GetGenericArguments();
                switch (genericArguments.Length)
                {
                    case 1: return genericArguments[0];
                    case 2 when type.IsDictionary(): return genericArguments[1];
                    default: throw CreateArgumentException();
                }
            }

            throw CreateArgumentException();

            Exception CreateArgumentException()
            {
                return new ArgumentException(
                    paramName: nameof(type),
                    message: $"The element type for {type.Name.QuoteWith("'")} could not be determined."
                );
            }
        }

        public static bool IsNullable([NotNull] this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsNullable<T>(this T value) => typeof(T).IsNullable();

        [CanBeNull]
        public static Type NullableType([NotNull] this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return
                type.IsNullable()
                    ? type.GetGenericArguments().Single()
                    : default;
        }

        public static bool TryGetNullableType([NotNull] this Type type, out Type valueType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            valueType =
                type.IsNullable()
                    ? type.GetGenericArguments().Single()
                    : default;

            return !(valueType is null);
        }
        
        /// <summary>
        /// Gets properties from all types in the hierarchy (also interfaces) so they might occur multiple times.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [NotNull, ItemNotNull]
        public static IEnumerable<PropertyInfo> GetPropertiesMany([NotNull] this Type type, BindingFlags bindingFlags)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            return
                from t in type.GetInterfaces().Prepend(type)
                from p in t.GetProperties(bindingFlags)
                select p;
        }
    }

    public static class Reflector<T>
    {
        public static bool HasDefaultConstructor() => typeof(T).HasDefaultConstructor();

        public static bool IsStatic() => typeof(T).IsStatic();

        public static bool Implements<TInterface>() => typeof(T).Implements(typeof(TInterface));

        public static bool IsEnumerable() => typeof(T).IsEnumerableOfT();

        public static bool IsEnumerableOf<TElement>() => typeof(T).IsEnumerableOf<TElement>();

        public static bool IsDictionary() => typeof(T).IsDictionary();

        public static bool IsList() => typeof(T).IsList();

        public static bool IsHashSet() => typeof(T).IsHashSet();

        public static IEnumerable<(Type Type, int Depth)> SelectMany() => typeof(T).SelectMany();

        //public static Type GetElementType()
    }
}