using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.OneTo1.Converters.Collections.Generic
{
    public class DictionaryToDictionary : TypeConverter
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsDictionary() && toType.IsDictionary();
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            var keyType = toType.GetGenericArguments()[0];
            var valueType = toType.GetGenericArguments()[1];

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var result = (IDictionary)Activator.CreateInstance(dictionaryType);

            var dictionary = (IDictionary)value;
            foreach (var key in dictionary.Keys)
            {
                result.Add
                (
                    context.Converter.Convert(key, keyType, context),
                    context.Converter.Convert(dictionary[key], valueType, context)
                );
            }

            return result;
        }
    }
}