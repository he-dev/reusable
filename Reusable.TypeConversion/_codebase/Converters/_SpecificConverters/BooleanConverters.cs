using System;

namespace Reusable.Converters.Converters
{
    public class StringToBooleanConverter : SpecificConverter<String, Boolean>
    {
        public override bool Convert(string value, ConversionContext context)
        {
            return bool.Parse(value);
        }
    }

    public class BooleanToStringConverter : SpecificConverter<bool, string>
    {
        public override string Convert(Boolean value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
