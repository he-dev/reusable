using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToInt64 : TypeConverter<String, Int64>
    {
        protected override long Convert(string value, ConversionContext context)
        {
            return Int64.Parse(value, NumberStyles.Integer, context.FormatProvider);
        }
    }

    public class Int64ToStringConverter : TypeConverter<long, string>
    {
        protected override string Convert(long value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}
