using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

namespace Reusable.OneTo1.Converters.Collections.Generic
{
    public class EnumerableToList : TypeConverter
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsEnumerable(except: typeof(string)) && toType.IsList();
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            var itemType = toType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var item in (IEnumerable)value)
            {
                var element = context.Converter.Convert(item, itemType, context);
                list.Add(element);
            }

            return list;
        }
    }
}