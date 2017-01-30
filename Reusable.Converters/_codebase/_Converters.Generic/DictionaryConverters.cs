using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.Converters
{
    public class DictionaryToDictionaryConverter : TypeConverter<IDictionary, object>
    {
        public override bool CanConvert(object value, Type targetType)
        {
            return value.GetType().IsDictionary() && targetType.IsDictionary();
        }

        protected override object ConvertCore(IConversionContext<IDictionary> context)
        {
            var keyType = context.TargetType.GetGenericArguments()[0];
            var valueType = context.TargetType.GetGenericArguments()[1];

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var result = (IDictionary)Activator.CreateInstance(dictionaryType);

            var dictionary = context.Value;
            foreach (var key in dictionary.Keys)
            {
                result.Add(
                    context.Converter.Convert(key, keyType, context.Format, context.FormatProvider),
                    context.Converter.Convert(dictionary[key], valueType, context.Format, context.FormatProvider));
            }

            return result;
        }
    }    
}