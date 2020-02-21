using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt64 : TypeConverter<String, UInt64>
    {
        protected override UInt64 Convert(string value, ConversionContext context)
        {
            return UInt64.Parse(value, context.FormatProvider);
        }
    }

    public class UInt64ToStringConverter : TypeConverter<ulong, string>
    {
        protected override string Convert(ulong value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}
