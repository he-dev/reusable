using System;
using System.Collections.Generic;
using System.Linq;
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
        
        
    }
}