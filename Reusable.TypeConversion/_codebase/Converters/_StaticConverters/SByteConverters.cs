using System;

namespace Reusable.Converters
{
    public class StringToSByteConverter : StaticConverter<String, SByte>
    {
        public override SByte Convert(string value, ConversionContext context)
        {
            return SByte.Parse(value, context.Culture);
        }
    }

    public class SByteToStringConverter : StaticConverter<sbyte, string>
    {
        public override string Convert(SByte value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
