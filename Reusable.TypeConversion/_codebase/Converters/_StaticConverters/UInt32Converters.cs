using System;

namespace Reusable.Converters
{
    public class StringToUInt32Converter : StaticConverter<String, UInt32>
    {
        public override UInt32 Convert(string value, ConversionContext context)
        {
            return UInt32.Parse(value, context.Culture);
        }
    }

    public class UInt32ToStringConverter : StaticConverter<uint, string>
    {
        public override string Convert(UInt32 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
