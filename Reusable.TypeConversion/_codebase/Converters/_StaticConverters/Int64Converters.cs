using System;

namespace Reusable.Converters
{
    public class StringToInt64Converter : StaticConverter<String, Int64>
    {
        public override Int64 Convert(string value, ConversionContext context)
        {
            return Int64.Parse(value, context.Culture);
        }
    }

    public class Int64ToStringConverter : StaticConverter<long, string>
    {
        public override string Convert(Int64 value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
