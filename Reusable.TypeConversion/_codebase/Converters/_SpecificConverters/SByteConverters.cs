using System;

namespace Reusable.Converters
{
    public class StringToSByteConverter : SpecificConverter<String, SByte>
    {
        public override SByte Convert(string value, ConversionContext context)
        {
            return SByte.Parse(value, context.Culture);
        }
    }

    public class SByteToStringConverter : SpecificConverter<sbyte, string>
    {
        public override string Convert(SByte value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
