using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.Converters
{
    public class DictionaryObjectObjectToDictionaryObjectObjectConverter : GenericConverter<IDictionary>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.IsDictionary() && arg.GetType().IsDictionary())
            {
                instance = Convert((IDictionary)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(IDictionary values, ConversionContext context)
        {
            var keyType = context.Type.GetGenericArguments()[0];
            var valueType = context.Type.GetGenericArguments()[1];

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

            foreach (var key in values.Keys)
            {
                dictionary.Add(
                    context.Service.Convert(key, keyType, context.Culture),
                    context.Service.Convert(values[key], valueType, context.Culture));
            }

            return dictionary;
        }
    }

    public class DictionaryObjectObjectToDictionaryStringStringConverter : GenericConverter<IDictionary>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            var isValidType =
                context.Type == typeof(Dictionary<string, string>) ||
                context.Type == typeof(IDictionary<string, string>);

            if (isValidType && arg.GetType().IsDictionary())
            {
                instance = Convert((IDictionary)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(IDictionary values, ConversionContext context)
        {
            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(string));
            var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

            foreach (var key in values.Keys)
            {
                dictionary.Add(
                    context.Service.Convert(key, typeof(string), context.Culture),
                    context.Service.Convert(values[key], typeof(string), context.Culture));
            }

            return dictionary;
        }
    }
}