using System;
using System.Collections;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.Converters
{
    public class EnumerableObjectToArrayObjectConverter : GenericConverter<IEnumerable>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.IsArray && arg.GetType().IsEnumerable())
            {
                instance = Convert((IEnumerable)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(IEnumerable values, ConversionContext context)
        {
            var elements = values.Cast<object>().ToArray();

            var elementType = context.Type.GetElementType();
            var array = Array.CreateInstance(elementType, elements.Length);

            for (var i = 0; i < elements.Length; i++)
            {
                array.SetValue(context.Service.Convert(elements[i], elementType), i);
            }

            return array;
        }
    }

    public class EnumerableObjectToArrayStringConverter : GenericConverter<IEnumerable>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.IsArray && arg.GetType().IsEnumerable())
            {
                instance = Convert((IEnumerable)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(IEnumerable values, ConversionContext context)
        {
            var elements = values.Cast<object>().ToArray();

            var elementType = context.Type.GetElementType();
            var array = Array.CreateInstance(elementType, elements.Length);

            for (var i = 0; i < elements.Length; i++)
            {
                array.SetValue(context.Service.Convert(elements[i], elementType), i);
            }

            return array;
        }
    }
}