using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.Converters
{
    public class DictionaryToDictionaryConverter : TypeConverter<IDictionary, object>
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            TypeConverterHelper.AssertNotNull(fromType, toType);
            return fromType.IsDictionary() && toType.IsDictionary();
        }

        protected override object ConvertCore(IConversionContext<IDictionary> context)
        {
            var keyType = context.ToType.GetGenericArguments()[0];
            var valueType = context.ToType.GetGenericArguments()[1];

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var result = (IDictionary) Activator.CreateInstance(dictionaryType);

            var dictionary = context.Value;
            foreach (var key in dictionary.Keys)
            {
                result.Add(
                    context.Converter.Convert(new ConversionContext<object>(key, keyType, context)),
                    context.Converter.Convert(new ConversionContext<object>(dictionary[key], valueType, context)));
            }

            return result;
        }
    }
}