using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Extensions;

public static class ObjectExtensions
{
    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNull<T>([NotNullWhen(false)] this T? value) where T : class => value is null;

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNull<T>([NotNullWhen(true)] this T? value) where T : class => value is { };


    public static IEnumerable<KeyValuePair<string, object?>> EnumerateProperties<T>(this T obj, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
    {
        if (obj is IDictionary<string, object?> dictionary)
        {
            return dictionary;
        }

        return
            from p in typeof(T).GetProperties(bindingFlags)
            select new KeyValuePair<string, object?>(p.Name, p.GetValue(obj));

    }
    
    public static IEnumerable<KeyValuePair<string, object?>> EnumerateProperties(this object? source, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
    {
        return source switch
        {
            null => Enumerable.Empty<KeyValuePair<string, object?>>(),
            IDictionary<string, object?> dictionary => dictionary,
            IImmutableDictionary<string, object?> dictionary => dictionary,
            _ => from p in source.GetType().GetProperties(bindingFlags) select new KeyValuePair<string, object?>(p.Name, p.GetValue(source))
        };
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
    
    // AsEnumerable is already taken and generates chars.
    public static IEnumerable<T> ToEnumerable<T>(this T source)
    {
        yield return source;
    }
}