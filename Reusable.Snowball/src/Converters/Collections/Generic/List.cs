using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Essentials.Extensions;

namespace Reusable.Snowball.Converters.Collections.Generic;

public class EnumerableToList : ITypeConverter
{
    public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
    {
        if (!value.GetType().IsEnumerable(except: typeof(string)) || !toType.IsList()) return default;

        context ??= new ConversionContext();

        var itemType = toType.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = (IList)Activator.CreateInstance(listType);

        foreach (var item in (IEnumerable)value)
        {
            var element = context.Converter.ConvertOrThrow(item, itemType);
            list.Add(element);
        }

        return list;
    }
}