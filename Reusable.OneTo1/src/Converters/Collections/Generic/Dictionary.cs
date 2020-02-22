using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.OneTo1.Converters.Specialized;
using Reusable.Reflection;

namespace Reusable.OneTo1.Converters.Collections.Generic
{
    public class DictionaryToDictionary : ITypeConverter
    {
        public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            if (!value.GetType().IsDictionary() || !toType.IsDictionary()) return default;

            context ??= new ConversionContext();
            
            var keyType = toType.GetGenericArguments()[0];
            var valueType = toType.GetGenericArguments()[1];

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var result = (IDictionary)Activator.CreateInstance(dictionaryType);

            var dictionary = (IDictionary)value;
            foreach (var key in dictionary.Keys)
            {
                result.Add
                (
                    context.Converter.ConvertOrThrow(key, keyType),
                    context.Converter.ConvertOrThrow(dictionary[key], valueType)
                );
            }

            return result;
        }
    }
}