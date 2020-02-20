using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDateTimeOffsetConverter : TypeConverter<String, DateTimeOffset>
    {
        protected override DateTimeOffset Convert(IConversionContext<string> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? DateTimeOffset.Parse(context.Value, context.FormatProvider, DateTimeStyles.None)
                    : DateTimeOffset.ParseExact(context.Value, context.Format, context.FormatProvider, DateTimeStyles.None);
        }
    }

    public class DateTimeOffsetToStringConverter : TypeConverter<DateTimeOffset, String>
    {
        protected override string Convert(IConversionContext<DateTimeOffset> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }   
}
