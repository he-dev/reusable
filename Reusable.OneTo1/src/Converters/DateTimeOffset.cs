using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDateTimeOffsetConverter : TypeConverter<String, DateTimeOffset>
    {
        protected override DateTimeOffset Convert(string value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(context.FormatString)
                    ? DateTimeOffset.Parse(value, context.FormatProvider, DateTimeStyles.None)
                    : DateTimeOffset.ParseExact(value, context.FormatString, context.FormatProvider, DateTimeStyles.None);
        }
    }

    public class DateTimeOffsetToStringConverter : TypeConverter<DateTimeOffset, String>
    {
        protected override string Convert(DateTimeOffset value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(context.FormatString)
                    ? value.ToString(context.FormatProvider)
                    : value.ToString(context.FormatString, context.FormatProvider);
        }
    }   
}
