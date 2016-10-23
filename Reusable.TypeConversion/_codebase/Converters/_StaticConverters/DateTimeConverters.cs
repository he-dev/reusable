using System;

namespace Reusable.Converters
{
    public class StringToDateTimeConverter : StaticConverter<String, DateTime>
    {
        public override DateTime Convert(string value, ConversionContext context)
        {
            return DateTime.Parse(value, context.Culture);
        }
    }

    public class DateTimeToStringConverter : StaticConverter<DateTime, String>
    {
        public override string Convert(DateTime value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
