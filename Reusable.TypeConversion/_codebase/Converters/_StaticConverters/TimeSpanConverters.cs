using System;

namespace Reusable.Converters
{
    public class StringToTimeSpanConverter : StaticConverter<String, TimeSpan>
    {
        public override TimeSpan Convert(string value, ConversionContext context)
        {
            return TimeSpan.Parse(value, context.Culture);
        }
    }

    public class TimeSpanToStringConverter : StaticConverter<TimeSpan, String>
    {
        public override string Convert(TimeSpan value, ConversionContext context)
        {
            return value.ToString();
        }
    }
}
