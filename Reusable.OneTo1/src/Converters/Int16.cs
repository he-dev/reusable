using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt16 : TypeConverter<String, Int16>
    {
        protected override short Convert(string value, ConversionContext context)
        {
            return Int16.Parse(value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int16ToStringConverter : TypeConverter<short, string>
    {
        protected override string Convert(short value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}