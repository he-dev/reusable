using System;

namespace Reusable.Converters
{
    public class StringToUInt64Converter : StaticConverter<String, UInt64>
    {
        public override UInt64 Convert(string value, ConversionContext context)
        {
            return UInt64.Parse(value, context.Culture);
        }
    }

    public class UInt64ToStringConverter : StaticConverter<ulong, string>
    {
        public override string Convert(UInt64 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
