using System;

namespace Reusable.Converters.Converters
{
    public class StringToInt16Converter : SpecificConverter<String, Int16>
    {
        public override Int16 Convert(string value, ConversionContext context)
        {
            return Int16.Parse(value, context.Culture);
        }
    }

    public class Int16ToStringConverter : SpecificConverter<short, string>
    {
        public override string Convert(Int16 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
