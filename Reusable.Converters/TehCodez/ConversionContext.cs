using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Reusable.Converters
{
    public interface IConversionContext<out TValue>
    {
        /// <summary>
        /// Value to convert.
        /// </summary>
        [NotNull]
        TValue Value { get; }
        
        [NotNull]
        Type FromType { get; }

        [NotNull]
        Type ToType { get; }

        [CanBeNull]
        string Format { get; }

        [CanBeNull]
        IFormatProvider FormatProvider { get; }

        [NotNull]
        ITypeConverter Converter { get; }
    }

    public class ConversionContext<TValue> : IConversionContext<TValue>
    {
        public ConversionContext([NotNull] TValue value, [NotNull] Type toType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (toType == null) throw new ArgumentNullException(nameof(toType));
            
            Value = value;
            ToType = toType;
        }
        public TValue Value { get; }
        public Type FromType => Value.GetType();
        public Type ToType { get; }
        public string Format { get; set; }
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        public ITypeConverter Converter { get; set; } = TypeConverter.Empty;

        [NotNull]
        public static ConversionContext<TValue> FromContext<T>(object value, Type toType, IConversionContext<T> other)
        {
            return new ConversionContext<TValue>((TValue)value, toType)
            {
                Format = other.Format,
                FormatProvider = other.FormatProvider,
                Converter = other.Converter,
            };
        }
    }
}