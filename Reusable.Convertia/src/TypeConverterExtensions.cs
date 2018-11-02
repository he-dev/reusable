using System;
using System.Diagnostics;

namespace Reusable.Convertia
{
    public static class TypeConverterExtensions
    {
        [DebuggerStepThrough]
        public static object Convert(this ITypeConverter converter, object value, Type toType, string format, IFormatProvider formatProvider)
        {
            return converter.Convert(new ConversionContext<object>(value, toType, converter)
            {
                Format = format,
                FormatProvider = formatProvider,
            });
        }

        [DebuggerStepThrough]
        public static object Convert(this ITypeConverter converter, object value, Type toType)
        {
            return converter.Convert(new ConversionContext<object>(value, toType, converter));
        }
    }
}