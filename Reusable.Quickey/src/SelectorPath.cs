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
    }
}