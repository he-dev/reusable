using System;
using System.Globalization;

namespace Reusable.Converters
{
    public interface IConversionContext<out TValue>
    {
        TValue Value { get; }
        Type TargetType { get; }
        string Format { get; }
        IFormatProvider FormatProvider { get; }
        TypeConverter Converter { get; }
    }

    public class ConversionContext<TValue> : IConversionContext<TValue>
    {
        public ConversionContext(TValue value, Type targetType, string format, IFormatProvider formatProvider, TypeConverter converter)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            Value = value;
            TargetType = targetType;
            Format = format;
            FormatProvider = formatProvider;
            Converter = converter;
        }

        public ConversionContext(TValue value, Type targetType, TypeConverter converter)
            : this(value, targetType, null, CultureInfo.InvariantCulture, converter)
        { }

        public ConversionContext(TValue value, Type targetType)
            : this(value, targetType, null, CultureInfo.InvariantCulture, TypeConverter.Empty)
        { }

        internal ConversionContext(IConversionContext<object> context)
            : this((TValue)context.Value, context.TargetType, context.Format, context.FormatProvider, context.Converter)
        {
        }

        public TValue Value { get; }
        public Type TargetType { get; }
        public string Format { get; set; }
        public IFormatProvider FormatProvider { get; }
        public TypeConverter Converter { get; }
    }
}
