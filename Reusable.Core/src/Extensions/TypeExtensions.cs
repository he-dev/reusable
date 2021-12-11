using System;
using Reusable.Exceptionize;

namespace Reusable.Extensions;

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
}