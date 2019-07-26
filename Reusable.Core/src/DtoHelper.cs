using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable
{
    public static class DtoHelper
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertiesWithSetter;

        static DtoHelper()
        {
            PropertiesWithSetter = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        }

        [NotNull]
        public static T Copy<T>([CanBeNull] this T source, [NotNull] Action<T> update) where T : class, new()
        {
            source = source ?? new T();

            var copy = new T();
            var propertiesWithSetter = PropertiesWithSetter.GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => Conditional.IsNotNull<MethodInfo>(p.GetSetMethod())));
            foreach (var property in propertiesWithSetter)
            {
                property.SetValue(copy, property.GetValue(source));
            }

            update(copy);

            return copy;
        }
    }
}