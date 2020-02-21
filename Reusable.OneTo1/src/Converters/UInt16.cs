using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToUInt16 : TypeConverter<String, UInt16>
    {
        protected override UInt16 Convert(string value, ConversionContext context)
        {
            return UInt16.Parse(value, context.FormatProvider);
        }
    }

    public class UInt16ToStringConverter : TypeConverter<ushort, string>
    {
        protected override string Convert(ushort value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}