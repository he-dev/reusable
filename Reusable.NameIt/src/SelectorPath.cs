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
            return member.Ancestors();
        }
        
        private static IEnumerable<MemberInfo> Ancestors(this MemberInfo member)
        {
            while(member is {})
            {
                yield return member;

                member =
                    // Follow the base class, otherwise the parent class.
                    member is Type type && type.IsNested && type.BaseType == typeof(object)
                        ? type.ReflectedType 
                        : member.Ancestor();
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