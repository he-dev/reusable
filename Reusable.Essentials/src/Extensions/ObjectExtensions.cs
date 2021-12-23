using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials.Data;

namespace Reusable.Essentials.Extensions;

public static class ObjectExtensions
{
    public static IEnumerable<Property> GetProperties<T>(this T obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
    {
        if (obj is IDictionary<string, object?> dictionary)
        {
            return dictionary.Select(x => new Property(x.Key, x.Value));
        }

        return
            from p in typeof(T).GetProperties(bindingFlags)
            select new Property(p.Name, p.GetValue(obj));

    }
        
//        public static IEnumerable<KeyValuePair<string, object>> EnumerateProperties<T>(this T obj)
//        {
//            if (obj is IDictionary<string, object> dictionary)
//            {
//                foreach (var item in dictionary)
//                {
//                    yield return item;
//                }
//
//                yield break;
//            }
//
//            if (!obj.GetType().Name.StartsWith("<>f__AnonymousType1"))
//            {
//                DynamicException.Factory.CreateDynamicException("ObjectType", "Object must be an anonymous type. Try new { ... }");
//            }
//
//            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
//            foreach (var property in properties)
//            {
//                yield return new KeyValuePair<string, object>(property.Name, property.GetValue(obj));
//            }
//        }

    public static Task<T> ToTask<T>(this T obj) => Task.FromResult<T>(obj);

    [Pure]
    [MustUseReturnValue]
    public static T DecorateWith<T>(this T decorable, Func<T, T> createDecorator)
    {
        return createDecorator(decorable);
    }
}