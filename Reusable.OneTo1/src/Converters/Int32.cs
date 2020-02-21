using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt32 : TypeConverter<String, Int32>
    {
        protected override int Convert(string value, ConversionContext context)
        {
            return Int32.Parse(value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int32ToStringConverter : TypeConverter<Int32, String>
    {
        protected override string Convert(Int32 value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}