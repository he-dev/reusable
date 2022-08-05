using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles.Reflection;

public static class Reflect<T>
{
    public static bool HasDefaultConstructor() => typeof(T).HasDefaultConstructor();

    public static bool IsStatic() => typeof(T).IsStatic();

    public static bool Implements<TInterface>() => typeof(T).Implements(typeof(TInterface));

    public static bool IsEnumerable() => typeof(T).IsEnumerable();

    public static bool IsEnumerableOf<TElement>() => typeof(T).IsEnumerableOf<TElement>();

    public static bool IsDictionary() => typeof(T).IsDictionary();

    public static bool IsList() => typeof(T).IsList();

    public static bool IsHashSet() => typeof(T).IsHashSet();

    public static IEnumerable<(Type Type, int Depth)> SelectMany() => typeof(T).SelectMany();

    public static IEnumerable<Type> GetTypesAssignableFrom()
    {
        return
            from t in typeof(T).Assembly.GetTypes()
            where t.IsClass && typeof(T).IsAssignableFrom(t)
            select t;
    }
}