using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Reusable.TypeConversion
{
    public class CompositeConverter : TypeConverter, IEnumerable<TypeConverter>
    {
        private readonly IEnumerable<TypeConverter> _converters;

        //internal CompositeConverter(IEnumerable<TypeConverter> converters)
        //{
        //    var compositeConverters = converters.Where(x => x != null).ToLookup(x => x is CompositeConverter);
        //    var currentConverters = compositeConverters[true].Cast<CompositeConverter>().SelectMany(x => x);
        //    var newConverters = compositeConverters[false];
        //    _converters.UnionWith(currentConverters);
        //    _converters.UnionWith(newConverters);
        //}

        //internal CompositeConverter(params TypeConverter[] converters) : this((IEnumerable<TypeConverter>)converters) { }

        internal CompositeConverter(params TypeConverter[] converters)
        {
            _converters = converters;
        }

        public override Type FromType => throw new NotSupportedException($"{nameof(CompositeConverter)} does not support {nameof(FromType)} property.");

        public override Type ToType => throw new NotSupportedException($"{nameof(CompositeConverter)} does not support {nameof(ToType)} property");

        public override bool CanConvert(object value, Type targetType)
        {
            return _converters.Any(x => x.CanConvert(value, targetType));
        }

        protected override object ConvertCore(IConversionContext<object> context)
        {
            if (IsConverted(context.Value, context.TargetType))
            {
                return context.Value;
            }

            var converter = FindConverter(context.Value, context.TargetType);

            return converter.Convert(
                new ConversionContext<object>(
                    context.Value,
                    context.TargetType,
                    context.Format,
                    context.FormatProvider,
                    this
                )
            );
        }

        private TypeConverter FindConverter(object value, Type targetType)
        {
            var converter = _converters.FirstOrDefault(x => x.CanConvert(value, targetType));
            if (converter == null)
            {
                throw new TypeConverterNotFoundException(value?.GetType(), targetType);
            }
            return converter;
        }

        public static CompositeConverter operator +(CompositeConverter left, TypeConverter right)
        {
            return new CompositeConverter(left, right);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TypeConverter> GetEnumerator()
        {
            return _converters.GetEnumerator();
        }
    }

    public class TypeConverterNotFoundException : Exception
    {
        public TypeConverterNotFoundException(Type valueType, Type targetType)
        {
            ValueType = valueType;
            TargetType = targetType;
        }

        public Type ValueType { get; }
        public Type TargetType { get; }

        public override string Message => $"Could not convert '{ValueType}' to '{TargetType}.";
    }
}
