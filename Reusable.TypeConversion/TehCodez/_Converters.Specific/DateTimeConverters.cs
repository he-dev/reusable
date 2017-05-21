using System;
using System.Globalization;

namespace Reusable.TypeConversion
{
    public class StringToDateTimeConverter : TypeConverter<String, DateTime>
    {
        protected override DateTime ConvertCore(IConversionContext<string> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? DateTime.Parse(context.Value, context.FormatProvider, DateTimeStyles.None)
                    : DateTime.ParseExact(context.Value, context.Format, context.FormatProvider, DateTimeStyles.None);
        }
    }

    public class DateTimeToStringConverter : TypeConverter<DateTime, String>
    {
        protected override string ConvertCore(IConversionContext<DateTime> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
