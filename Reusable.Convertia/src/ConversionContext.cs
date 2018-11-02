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

        [NotNull]
        string Format { get; }

        [NotNull]
        IFormatProvider FormatProvider { get; }

        [NotNull]
        ITypeConverter Converter { get; }
    }

    public class ConversionContext<TValue> : IConversionContext<TValue>
    {
        private string _format;
        private IFormatProvider _formatProvider;

        private ConversionContext()
        {
            Format = TypeConverter.DefaultFormat;
            FormatProvider = TypeConverter.DefaultFormatProvider;
        }

        public ConversionContext([NotNull] TValue value, [NotNull] Type toType, [NotNull] ITypeConverter converter) 
            : this()
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (toType == null) throw new ArgumentNullException(nameof(toType));

            Value = value;
            ToType = toType;
            Converter = converter;
        }

        public ConversionContext([NotNull] TValue value, [NotNull] Type toType)
            : this(value, toType, TypeConverter.Empty)
        { }

        public ConversionContext([NotNull] IConversionContext<TValue> context)
            : this(context.Value, context.ToType, context.Converter)
        {
            Format = context.Format;
            FormatProvider = context.FormatProvider;
        }

        public ConversionContext([NotNull] TValue value, [NotNull] Type toType, [NotNull] IConversionContext<TValue> context)
        : this(value, toType, context.Converter)
        {
            Format = context.Format;
            FormatProvider = context.FormatProvider;
        }

        public TValue Value { get; }

        public Type FromType => Value.GetType();

        public Type ToType { get; }

        public ITypeConverter Converter { get; }

        public string Format
        {
            get => _format;
            set => _format = value ?? throw new ArgumentNullException(nameof(Format));
        }

        public IFormatProvider FormatProvider
        {
            get => _formatProvider;
            set => _formatProvider = value ?? throw new ArgumentNullException(nameof(FormatProvider));
        }
    }
}