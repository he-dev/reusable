using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToSByte : TypeConverter<String, SByte>
    {
        protected override SByte Convert(string value, ConversionContext context)
        {
            return SByte.Parse(value, context.FormatProvider);
        }
    }

    public class SByteToStringConverter : TypeConverter<sbyte, string>
    {
        protected override string Convert(sbyte value, ConversionContext context)
        {
            return value.ToString(context.FormatProvider);
        }
    }
}
