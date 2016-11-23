using System;

namespace Reusable.Converters.Converters
{
    public class StringToTimeSpanConverter : SpecificConverter<String, TimeSpan>
    {
        public override TimeSpan Convert(string value, ConversionContext context)
        {
            return TimeSpan.Parse(value, context.Culture);
        }
    }

    public class TimeSpanToStringConverter : SpecificConverter<TimeSpan, String>
    {
        public override string Convert(TimeSpan value, ConversionContext context)
        {
            return value.ToString();
        }
    }
}
