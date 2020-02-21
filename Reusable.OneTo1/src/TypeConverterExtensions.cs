using System;
using System.Diagnostics;

namespace Reusable.OneTo1
{
    public static class TypeConverterExtensions
    {
        [DebuggerStepThrough]
        public static object Convert(this ITypeConverter converter, object value, Type toType, string format, IFormatProvider formatProvider)
        {
            return converter.Convert(value, toType, new ConversionContext
            {
                Converter = converter,
                FormatString = format,
                FormatProvider = formatProvider
            });
        }

        [DebuggerStepThrough]
        public static object Convert(this ITypeConverter converter, object value, Type toType)
        {
            return converter.Convert(value, toType, new ConversionContext
            {
                Converter = converter
            });
        }

        [DebuggerStepThrough]
        public static T Convert<T>(this ITypeConverter converter, object value)
        {
            return (T)converter.Convert(value, typeof(T), new ConversionContext
            {
                Converter = converter
            });
        }
    }
}