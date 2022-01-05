using System;
using System.Collections.Generic;
using System.Reflection;

namespace Reusable.ReMember;

public static class SelectorPath
{
    /// <summary>
    /// Enumerates selector path in the class hierarchy from member to parent.
    /// </summary>
    public static IEnumerable<MemberInfo> Ancestors(this MemberInfo member)
    {
        var current = member;
        
        while (current is { })
        {
            yield return current;

            current =
                // Follow the base class, otherwise the parent class.
                current is Type { IsNested: true } type && type.BaseType == typeof(object)
                    ? type.ReflectedType
                    : current.Ancestor();
        }
    }

    private static MemberInfo? Ancestor(this MemberInfo member)
    {
        return member switch
        {
            PropertyInfo p => p.ReflectedType,
            FieldInfo f => f.ReflectedType,
            Type t => t switch
            {
                { IsInterface: true } => default,
                _ => t.BaseType switch
                {
                    { } b when b == typeof(object) => default,
                    { } b => b,
                    _ => default
                }
            },
            _ => default
        };
    }
}