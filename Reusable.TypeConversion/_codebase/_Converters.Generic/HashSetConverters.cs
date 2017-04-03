using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Extensions;

namespace Reusable.TypeConversion
{
    public class EnumerableToHashSetConverter : TypeConverter<IEnumerable, object>
    {
        public override bool CanConvert(object value, Type targetType)
        {
            return value.GetType().IsEnumerable() && targetType.IsHashSet();
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var valueType = context.TargetType.GetGenericArguments()[0];
            var hashSetType = typeof(HashSet<>).MakeGenericType(valueType);
            var hashSet = Activator.CreateInstance(hashSetType);
            var addMethod = hashSetType.GetMethod(nameof(HashSet<object>.Add));

            foreach (var value in context.Value)
            {
                var element = context.Converter.Convert(value, valueType, context.Format, context.FormatProvider);
                addMethod.Invoke(hashSet, new[] { element });
            }

            return hashSet;
        }
    }
}