using System;
using System.Collections;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.OneTo1.Converters.Collections
{
    public class EnumerableToArray : TypeConverter
    {
        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsEnumerable(except: typeof(string)) && toType.IsArray;
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            var elements = ((IEnumerable)value).Cast<object>().ToArray();
            var elementType = toType.GetElementType() ?? throw DynamicException.Factory.CreateDynamicException($"ElementTypeNotFound{nameof(Exception)}", $"Could not determine the element type of {toType.ToPrettyString()}.", null);
            var array = Array.CreateInstance(elementType, elements.Length);

            for (var i = 0; i < elements.Length; i++)
            {
                var item = context.Converter.Convert(elements[i], elementType, context);
                array.SetValue(item, i);
            }

            return array;
        }
    }
}