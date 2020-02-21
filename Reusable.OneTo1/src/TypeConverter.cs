using System;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.OneTo1
{
    public interface ITypeConverter
    {
        bool CanConvert(Type fromType, Type toType);

        object Convert(object value, Type toType, ConversionContext? context = default);
    }

    public abstract class TypeConverter : ITypeConverter
    {
        public static ITypeConverter Empty => new CompositeConverter();

        public static ITypeConverter PassThru { get; } = new passThru();

        public abstract bool CanConvert(Type fromType, Type toType);

        public virtual object Convert(object value, Type toType, ConversionContext? context = default)
        {
            if (CanConvert(value.GetType(), toType))
            {
                if (toType.IsInstanceOfType(value))
                {
                    return value;
                }
                else
                {
                    try
                    {
                        return ConvertImpl(value, toType, context ?? new ConversionContext());
                    }
                    catch (Exception inner)
                    {
                        throw DynamicException.Create
                        (
                            $"Conversion",
                            $"Could not convert from '{value.GetType().ToPrettyString()}' to '{toType.ToPrettyString()}'.",
                            inner
                        );
                    }
                }
            }
            else
            {
                throw NotSupportedConversion(value.GetType(), toType);
            }
        }

        protected abstract object ConvertImpl(object value, Type toType, ConversionContext context);


        protected static Exception NotSupportedConversion(Type fromType, Type toType)
        {
            throw DynamicException.Create
            (
                $"NotSupportedConversion",
                $"There is no converter from '{fromType.ToPrettyString()}' to '{toType.ToPrettyString()}'."
            );
        }

        private class passThru : ITypeConverter
        {
            public bool CanConvert(Type fromType, Type toType) => true;

            public object Convert(object value, Type toType, ConversionContext? context = default) => value;
        }
    }

    public abstract class TypeConverter<TValue, TResult> : TypeConverter
    {
        private static Type FromType => typeof(TValue);

        private static Type ToType => typeof(TResult);

        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType == FromType && toType == ToType;
        }

        protected override object ConvertImpl(object value, Type toType, ConversionContext context)
        {
            return Convert((TValue)value, context ?? new ConversionContext());
        }

        protected abstract TResult Convert(TValue value, ConversionContext context);
    }

    [PublicAPI]
    public abstract class ToStringConverter<TValue> : TypeConverter<TValue, string>
    {
        public string FormatString { get; set; }

        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
    }

    [PublicAPI]
    public abstract class FromStringConverter<TResult> : TypeConverter<string, TResult>
    {
        public string? FormatString { get; set; }

        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
    }
}