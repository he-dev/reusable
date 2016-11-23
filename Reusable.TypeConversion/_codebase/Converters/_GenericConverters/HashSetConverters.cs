using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Converters.Converters
{
    public class EnumerableObjectToHashSetObjectConverter : GenericConverter<IEnumerable>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.IsHashSet() && arg.GetType().IsEnumerable())
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

            var hashSetType = typeof(HashSet<>).MakeGenericType(valueType);
            var hashSet = Activator.CreateInstance(hashSetType);
            var addMethod = hashSetType.GetMethod("Add");

            foreach (var value in values)
            {
                addMethod.Invoke(hashSet, new[]
                {
                    context.Service.Convert(value, valueType, context.Culture)
                });
            }

            return hashSet;
        }
    }

    public class EnumerableObjectToHashSetStringConverter : GenericConverter<IEnumerable>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.IsHashSet() && arg.GetType().IsEnumerable())
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

            var hashSetType = typeof(HashSet<>).MakeGenericType(valueType);
            var hashSet = Activator.CreateInstance(hashSetType);
            var addMethod = hashSetType.GetMethod("Add");

            foreach (var value in values)
            {
                addMethod.Invoke(hashSet, new[]
                {
                    context.Service.Convert(value, valueType, context.Culture)
                });
            }

            return hashSet;
        }
    }
}