using System;

namespace Reusable.Converters.Converters
{
    public class StringToDoubleConverter : SpecificConverter<String, Double>
    {
        public override Double Convert(string value, ConversionContext context)
        {
            return Double.Parse(value, context.Culture);
        }
    }

    public class DoubleToStringConverter : SpecificConverter<double, string>
    {
        public override string Convert(Double value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
