using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Extensions;

namespace Reusable.Converters
{
    public class EnumerableObjectToListObjectConverter : DynamicConverter<IEnumerable>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.IsList() && arg.GetType().IsEnumerable())
            {
                instance = Convert((IEnumerable)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(IEnumerable values, ConversionContext context)
        {
            var valueType = context.Type.GetGenericArguments()[0];

            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var value in values)
            {
                list.Add(context.Service.Convert(value, valueType));
            }

            return list;
        }
    }

    public class EnumerableObjectToListStringConverter : DynamicConverter<IEnumerable>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            var isValidType = context.Type == typeof(List<string>) || context.Type == typeof(IList<string>);
            if (isValidType && arg.GetType().IsEnumerable())
            {
                instance = Convert((IEnumerable)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(IEnumerable values, ConversionContext context)
        {
            var valueType = context.Type.GetGenericArguments()[0];

            var listType = typeof(List<>).MakeGenericType(valueType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var value in values)
            {
                list.Add(context.Service.Convert(value, valueType));
            }

            return list;
        }
    }
}