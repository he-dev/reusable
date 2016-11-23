using System;

namespace Reusable.Converters.Converters
{
    public class StringToSingleConverter : SpecificConverter<String, Single>
    {
        public override Single Convert(string value, ConversionContext context)
        {
            return Single.Parse(value, context.Culture);
        }
    }

    public class SingleToStringConverter : SpecificConverter<float, string>
    {
        public override string Convert(Single value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
