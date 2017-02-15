using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.TypeConversion
{
    public class EnumerableToListConverter : TypeConverter<IEnumerable, object>
    {
        public override bool CanConvert(object value, Type targetType)
        {
            return value.GetType().IsEnumerable() && targetType.IsList();
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var valueType = context.TargetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var value in context.Value)
            {
                var element = context.Converter.Convert(value, valueType, context.Format, context.FormatProvider);
                list.Add(element);
            }

            return list;
        }
    }
}