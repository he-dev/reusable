using System;

namespace Reusable.Converters.Converters
{
    public class StringToByteConverter : SpecificConverter<String, Byte>
    {
        public override Byte Convert(string value, ConversionContext context)
        {
            return Byte.Parse(value, (IFormatProvider)context.Culture);
        }
    }

    public class ByteToStringConverter : SpecificConverter<byte, string>
    {
        public override string Convert(Byte value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
