using System;

namespace Reusable.Converters
{
    public class StringToSingleConverter : StaticConverter<String, Single>
    {
        public override Single Convert(string value, ConversionContext context)
        {
            return Single.Parse(value, context.Culture);
        }
    }

    public class SingleToStringConverter : StaticConverter<float, string>
    {
        public override string Convert(Single value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
