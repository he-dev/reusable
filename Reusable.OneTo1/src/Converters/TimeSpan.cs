using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToTimeSpan : TypeConverter<String, TimeSpan>
    {
        protected override TimeSpan Convert(string value, ConversionContext context)
        {
            return TimeSpan.Parse(value, context.FormatProvider);
        }
    }

    public class TimeSpanToStringConverter : TypeConverter<TimeSpan, String>
    {
        protected override string Convert(TimeSpan value, ConversionContext context)
        {
            return value.ToString();
        }
    }
}
