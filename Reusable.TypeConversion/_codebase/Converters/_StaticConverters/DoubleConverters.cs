using System;

namespace Reusable.Converters
{
    public class StringToDoubleConverter : StaticConverter<String, Double>
    {
        public override Double Convert(string value, ConversionContext context)
        {
            return Double.Parse(value, context.Culture);
        }
    }

    public class DoubleToStringConverter : StaticConverter<double, string>
    {
        public override string Convert(Double value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
