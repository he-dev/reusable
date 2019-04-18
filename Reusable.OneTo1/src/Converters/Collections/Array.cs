using System;
using System.Collections;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Reflection;

namespace Reusable.OneTo1.Converters.Collections
{
    public class EnumerableToArrayConverter : TypeConverter<IEnumerable, object>
    {
        protected override bool CanConvertCore(Type fromType, Type toType)
        {
            return fromType.IsEnumerableOfT(except: typeof(string)) && toType.IsArray;
        }

        protected override object ConvertCore(IConversionContext<IEnumerable> context)
        {
            var elements = context.Value.Cast<object>().ToArray();

            var elementType = context.ToType.GetElementType();
            if (elementType is null)
            {
                throw DynamicException.Factory.CreateDynamicException($"ElementTypeNotFound{nameof(Exception)}", $"Could not determine the element type of {context.ToType.Name}.", null);
            }

            var array = Array.CreateInstance(elementType, elements.Length);

            for (var i = 0; i < elements.Length; i++)
            {
                //var value = context.Converter.Convert(ConversionContext<object>.FromContext(elements[i], elementType, context));
                var value = context.Converter.Convert(new ConversionContext<object>(elements[i], elementType, context));
                array.SetValue(value, i);
            }

            return array;
        }
    }
}