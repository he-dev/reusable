using System;

namespace Reusable.Converters.Converters
{
    public class StringToUInt16Converter : SpecificConverter<String, UInt16>
    {
        public override UInt16 Convert(string value, ConversionContext context)
        {
            return UInt16.Parse(value, context.Culture);
        }
    }

    public class UInt16ToStringConverter : SpecificConverter<ushort, string>
    {
        public override string Convert(UInt16 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
