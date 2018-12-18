using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToTimeSpanConverter : TypeConverter<String, TimeSpan>
    {
        protected override TimeSpan ConvertCore(IConversionContext<String> context)
        {
            return TimeSpan.Parse(context.Value, context.FormatProvider);
        }
    }

    public class TimeSpanToStringConverter : TypeConverter<TimeSpan, String>
    {
        protected override string ConvertCore(IConversionContext<TimeSpan> context)
        {
            return context.Value.ToString();
        }
    }
}
