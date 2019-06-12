using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Reflection
{
    public static class Utilities
    {
        public static MemberInfo FindProperty([NotNull] this Type type, [NotNull] string name)
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

        public static IEnumerable<T> FindCustomAttributes<T>([NotNull] this MemberInfo info) where T : Attribute
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

        public static IEnumerable<(T Attribute, IEnumerable<string> Path)> EnumerateCustomAttributes<T>([NotNull] this PropertyInfo property) where T : Attribute
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var queue = new Queue<(MemberInfo Member, IEnumerable<string> Path)>
            {
                (property, new[] { property.Name }),
                (property.DeclaringType, new[] { property.Name, property.DeclaringType.Name }),
            };

            while (queue.Any())
            {
                var (member, path) = queue.Dequeue();

                foreach (var attribute in member.GetCustomAttributes<T>())
                {
                    yield return (attribute, path);
                }

                if (member is Type type)
                {
                    var baseTypes = type.GetInterfaces().AsEnumerable();

                    if (type.IsSubclass())
                    {
                        baseTypes = baseTypes.Prepend(type.BaseType);
                    }

                    foreach (var i in baseTypes)
                    {
                        queue.Enqueue((i, path.Append(i.Name)));
                    }
                }
            }
        }

        public static bool IsSubclass(this Type type)
        {
            return type.IsClass && type.BaseType != typeof(object);
        }
    }
}