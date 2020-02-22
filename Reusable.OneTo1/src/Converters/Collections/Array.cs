using System;
using System.Collections;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OneTo1.Converters.Specialized;
using Reusable.Reflection;

namespace Reusable.OneTo1.Converters.Collections
{
    public class EnumerableToArray : ITypeConverter
    {
        public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            if (!value.GetType().IsEnumerable(except: typeof(string)) || !toType.IsArray) return default;

            context ??= new ConversionContext();
            
            var elements = ((IEnumerable)value).Cast<object>().ToArray();
            var elementType = toType.GetElementType() ?? throw DynamicException.Factory.CreateDynamicException($"ElementTypeNotFound{nameof(Exception)}", $"Could not determine the element type of {toType.ToPrettyString()}.", null);
            var array = Array.CreateInstance(elementType, elements.Length);

            for (var i = 0; i < elements.Length; i++)
            {
                var item = context.Converter.ConvertOrThrow(elements[i], elementType);
                array.SetValue(item, i);
            }

            return array;
        }
    }
}