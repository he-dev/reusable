using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

namespace Reusable.OneTo1.Converters.Collections.Generic
{
    public class EnumerableToListConverter : TypeConverter<IEnumerable, object>
    {
        protected override bool CanConvertCore(Type fromType, Type toType)
        {
            return fromType.IsEnumerableOfT(except: typeof(string)) && toType.IsList();
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var valueType = context.ToType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = (IList) Activator.CreateInstance(listType);

            foreach (var value in context.Value)
            {
                var element = context.Converter.Convert(new ConversionContext<object>(value, valueType, context));
                list.Add(element);
            }

            return list;
        }
    }
}