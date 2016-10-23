using System;

namespace Reusable.Converters
{
    public class StringToInt16Converter : StaticConverter<String, Int16>
    {
        public override Int16 Convert(string value, ConversionContext context)
        {
            return Int16.Parse(value, context.Culture);
        }
    }

    public class Int16ToStringConverter : StaticConverter<short, string>
    {
        public override string Convert(Int16 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
