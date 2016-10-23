using System;

namespace Reusable.Converters
{
    public class StringToBooleanConverter : StaticConverter<String, Boolean>
    {
        public override bool Convert(string value, ConversionContext context)
        {
            return bool.Parse(value);
        }
    }

    public class BooleanToStringConverter : StaticConverter<bool, string>
    {
        public override string Convert(Boolean value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
