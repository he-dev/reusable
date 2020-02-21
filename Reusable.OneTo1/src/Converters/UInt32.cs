using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt32 : TypeConverter<String, UInt32>
    {
        protected override UInt32 Convert(string value, ConversionContext context)
        {
            return UInt32.Parse(value, context.FormatProvider);
        }
    }

    public class UInt32ToStringConverter : TypeConverter<uint, string>
    {
        protected override string Convert(uint value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}
