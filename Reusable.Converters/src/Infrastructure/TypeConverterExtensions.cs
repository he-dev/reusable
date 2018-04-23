using System;

namespace Reusable.Converters
{
    public static class TypeConverterExtensions
    {
        public static object Convert(this ITypeConverter converter, object value, Type toType, string format, IFormatProvider formatProvider)
        {
            return converter.Convert(new ConversionContext<object>(value, toType)
            {
                Format = format,
                FormatProvider = formatProvider,
                Converter = converter
            });
        }

        public static object Convert(this ITypeConverter converter, object value, Type toType)
        {
            return converter.Convert(new ConversionContext<object>(value, toType)
            {
                Converter = converter
            });
        }
    }
}