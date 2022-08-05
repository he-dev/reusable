using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.Marbles.Extensions;

public static class TypeExtensions
{
    public static bool IsAnonymous<T>(this T obj)
    {
        return typeof(T).IsAnonymous();
    }

    public static bool IsAnonymous(this Type type)
    {
        return type.Name.StartsWith("<>f__AnonymousType");
    }

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

    public static bool IsEnumerable(this Type type, params Type[] except)
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

    public static Type GetElementType(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsArray)
        {
            return type.GetElementType() ?? throw CreateArgumentException();
        }

        if (type.IsEnumerable())
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

    public static bool IsNullable(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        return
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsNullable<T>(this T value) => typeof(T).IsNullable();

    [CanBeNull]
    public static Type NullableType(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        return
            type.IsNullable()
                ? type.GetGenericArguments().Single()
                : default;
    }

    public static bool TryGetNullableType(this Type type, out Type valueType)
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
    public static IEnumerable<PropertyInfo> GetPropertiesMany(this Type type, BindingFlags bindingFlags)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        return
            from t in type.GetInterfaces().Prepend(type)
            from p in t.GetProperties(bindingFlags)
            select p;
    }

    public static MemberInfo FindProperty(this Type type, string name)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (name == null) throw new ArgumentNullException(nameof(name));

        if (!type.IsInterface) throw new ArgumentException(paramName: nameof(type), message: $"'{nameof(type)}' must be an interface.");

        var queue = new Queue<Type>()
        {
            type
        };

        while (queue.Any())
        {
            type = queue.Dequeue();
            if (type.GetProperty(name) is var p && !(p is null))
            {
                return p;
            }

            foreach (var i in type.GetInterfaces())
            {
                queue.Enqueue(i);
            }
        }

        return default;
    }

    public static IEnumerable<T> FindCustomAttributes<T>(this MemberInfo info) where T : Attribute
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        var queue = new Queue<MemberInfo>()
        {
            info
        };

        while (queue.Any())
        {
            info = queue.Dequeue();

            foreach (var a in info.GetCustomAttributes<T>())
            {
                yield return a;
            }

            if (info is Type type)
            {
                if (type.IsClass && !(type.BaseType == null))
                {
                    queue.Enqueue(type.BaseType);
                }

                foreach (var i in type.GetInterfaces())
                {
                    queue.Enqueue(i);
                }
            }
        }
    }
}