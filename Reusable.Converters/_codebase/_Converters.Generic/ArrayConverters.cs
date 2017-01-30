using System;
using System.Collections;
using System.Linq;

namespace Reusable.Converters
{
    public class EnumerableToArrayConverter : TypeConverter<IEnumerable, object>
    {
        public override bool CanConvert(object value, Type targetType)
        {
            return targetType.IsArray && value.GetType().IsEnumerable();
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var elements = context.Value.Cast<object>().ToArray();

            var elementType = context.TargetType.GetElementType();
            var array = Array.CreateInstance(elementType, elements.Length);

            for (var i = 0; i < elements.Length; i++)
            {
                var value = context.Converter.Convert(elements[i], elementType, context.Format, context.FormatProvider);
                array.SetValue(value, i);
            }

            return array;
        }
    }
}