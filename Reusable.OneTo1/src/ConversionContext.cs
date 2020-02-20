using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace Reusable.OneTo1
{
    public interface IConversionContext<out TValue>
    {
        /// <summary>
        /// Value to convert.
        /// </summary>
        [NotNull]
        TValue Value { get; }

        Type FromType { get; }

        Type ToType { get; }

        bool SameTypes { get; }

        string? Format { get; }

        IFormatProvider FormatProvider { get; }

        ITypeConverter Converter { get; }
    }

    public class ConversionContext<TValue> : IConversionContext<TValue>
    {
        private IFormatProvider _formatProvider;

        [DebuggerStepThrough]
        private ConversionContext()
        {
            FormatProvider = CultureInfo.InvariantCulture;
        }

        [DebuggerStepThrough]
        public ConversionContext([NotNull] TValue value, Type toType, ITypeConverter converter)
            : this()
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            Value = value;
            ToType = toType ?? throw new ArgumentNullException(nameof(toType));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public ConversionContext([NotNull] TValue value, Type toType, IConversionContext<TValue> context)
            : this(value, toType, context.Converter)
        {
            Format = context.Format;
            FormatProvider = context.FormatProvider;
        }

        public TValue Value { get; }

        public Type FromType => Value.GetType();

        public Type ToType { get; }

        public bool SameTypes => FromType == ToType;

        public ITypeConverter Converter { get; }

        public string Format { get; set; }

        public IFormatProvider FormatProvider
        {
            get => _formatProvider;
            set => _formatProvider = value ?? throw new ArgumentNullException(nameof(FormatProvider));
        }
    }
}