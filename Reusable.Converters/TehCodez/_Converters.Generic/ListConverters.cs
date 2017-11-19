using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

namespace Reusable.Converters
{
    public class EnumerableToListConverter : TypeConverter<IEnumerable, object>
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsEnumerable() && toType.IsList();
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var valueType = context.ToType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = (IList) Activator.CreateInstance(listType);

            foreach (var value in context.Value)
            {
                var element = context.Converter.Convert(ConversionContext<object>.FromContext(value, valueType, context));
                list.Add(element);
            }

            return list;
        }
    }
}