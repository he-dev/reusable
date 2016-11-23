using System;

namespace Reusable.Converters
{
    public class StringToInt32Converter : SpecificConverter<String, Int32>
    {
        public override Int32 Convert(string value, ConversionContext context)
        {
            return Int32.Parse(value, context.Culture);
        }
    }

    public class Int32ToStringConverter : SpecificConverter<Int32, String>
    {
        public override string Convert(Int32 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
