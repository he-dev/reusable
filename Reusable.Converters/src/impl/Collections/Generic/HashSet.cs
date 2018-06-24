using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

namespace Reusable.Converters.Collections.Generic
{
    public class EnumerableToHashSetConverter : TypeConverter<IEnumerable, object>
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsEnumerable() && toType.IsHashSet();
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var valueType = context.ToType.GetGenericArguments()[0];
            var hashSetType = typeof(HashSet<>).MakeGenericType(valueType);
            var hashSet = Activator.CreateInstance(hashSetType);
            var addMethod = hashSetType.GetMethod(nameof(HashSet<object>.Add));

            foreach (var value in context.Value)
            {
                var element = context.Converter.Convert(new ConversionContext<object>(value, valueType, context));
                // ReSharper disable once PossibleNullReferenceException - this is never null
                addMethod.Invoke(hashSet, new[] {element});
            }

            return hashSet;
        }
    }
}