using System;
using System.Collections.Generic;
using System.Reflection;

namespace Reusable.Quickey
{
    public static class SelectorPath
    {
        /// <summary>
        /// Enumerates selector path in the class hierarchy from member to parent.
        /// </summary>
        public static IEnumerable<MemberInfo> Enumerate(MemberInfo member)
        {
            return member.AncestorTypesAndSelf();
        }

        private static IEnumerable<MemberInfo> AncestorTypesAndSelf(this MemberInfo member)
        {
            for (var current = member; current is {};)
            {
                //if (current.IsDefined(typeof()))
                yield return current;

                switch (current)
                {
                    case FieldInfo field:
                        current = field.ReflectedType;
                        break;

                    case PropertyInfo property:
                        current = property.ReflectedType;
                        break;

                    case Type type:

                        // Don't follow interfaces.
                        if (type.IsInterface)
                        {
                            yield break;
                        }

                        type = type.BaseType;

                        // Don't follow object because it's the base type for everything.
                        if (type is null || type == typeof(object))
                        {
                            yield break;
                        }

                        // Follow properties.
                        if (type.GetProperty(member.Name) is { } otherProperty)
                        {
                            yield return otherProperty;
                        }

                        current = type;
                        break;
                }
            }
        }

        private static IEnumerable<MemberInfo> AncestorTypesAndSelf2(this MemberInfo member)
        {
            yield return member;
            for (; member is {}; member = member.Ancestor())
            {
                yield return member;
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
                    {IsInterface: true} => default,
                    _ => t.BaseType switch
                    {
                        {} b when b == typeof(object) => default,
                        {} b => b,
                        _ => default
                    }
                },
                _ => default
            };
        }
    }
}