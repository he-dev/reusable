using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Marbles.Extensions;

namespace Reusable.Snowball.Converters.Collections.Generic;

public class EnumerableToHashSet : ITypeConverter
{
    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
    {
        if (!value.GetType().IsEnumerable(except: typeof(string)) || !toType.IsHashSet()) return default;
            
        context ??= new ConversionContext();
            
        var itemType = toType.GetGenericArguments()[0];
        var hashSetType = typeof(HashSet<>).MakeGenericType(itemType);
        var hashSet = Activator.CreateInstance(hashSetType);
        var addMethod = hashSetType.GetMethod(nameof(HashSet<object>.Add));

        foreach (var item in (IEnumerable)value)
        {
            var element = context.Converter.ConvertOrThrow(item, itemType);
            // ReSharper disable once PossibleNullReferenceException - addMethod is never null
            addMethod.Invoke(hashSet, new[] { element });
        }

        return hashSet;
    }
}