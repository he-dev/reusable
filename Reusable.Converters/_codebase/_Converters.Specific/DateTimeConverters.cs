using System;

namespace Reusable.Converters
{
    public class StringToDateTimeConverter : SpecificConverter<String, DateTime>
    {
        public override DateTime Convert(string value, ConversionContext context)
        {
            return DateTime.Parse(value, context.Culture);
        }
    }

    public class DateTimeToStringConverter : SpecificConverter<DateTime, String>
    {
        public override string Convert(DateTime value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
