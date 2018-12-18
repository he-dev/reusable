using System;
using System.Diagnostics;
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

        [NotNull]
        Type FromType { get; }

        [NotNull]
        Type ToType { get; }

        [CanBeNull]
        string Format { get; }

        [NotNull]
        IFormatProvider FormatProvider { get; }

        [NotNull]
        ITypeConverter Converter { get; }
    }

    public class ConversionContext<TValue> : IConversionContext<TValue>
    {
        private IFormatProvider _formatProvider;

        [DebuggerStepThrough]
        private ConversionContext()
        {
            //_format = TypeConverter.DefaultFormat;
            FormatProvider = TypeConverter.DefaultFormatProvider;
        }

        [DebuggerStepThrough]
        public ConversionContext([NotNull] TValue value, [NotNull] Type toType, [NotNull] ITypeConverter converter) 
            : this()
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            Value = value;
            ToType = toType ?? throw new ArgumentNullException(nameof(toType));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        //public ConversionContext([NotNull] TValue value, [NotNull] Type toType)
        //    : this(value, toType, TypeConverter.Empty)
        //{ }

        //public ConversionContext([NotNull] IConversionContext<TValue> context)
        //    : this(context.Value, context.ToType, context.Converter)
        //{
        //    Format = context.Format;
        //    FormatProvider = context.FormatProvider;
        //}

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

        public string Format { get; set; }
        //{
        //    get => _format;
        //    set => _format = value ?? throw new ArgumentNullException(nameof(Format));
        //}

        public IFormatProvider FormatProvider
        {
            get => _formatProvider;
            set => _formatProvider = value ?? throw new ArgumentNullException(nameof(FormatProvider));
        }
    }
}