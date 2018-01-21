using System;
using System.Globalization;
using System.Text;

namespace Reusable.Converters
{
    public class StringToDateTimeOffsetConverter : TypeConverter<String, DateTimeOffset>
    {
        protected override DateTimeOffset ConvertCore(IConversionContext<string> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? DateTimeOffset.Parse(context.Value, context.FormatProvider, DateTimeStyles.None)
                    : DateTimeOffset.ParseExact(context.Value, context.Format, context.FormatProvider, DateTimeStyles.None);
        }
    }

    public class DateTimeOffsetToStringConverter : TypeConverter<DateTimeOffset, String>
    {
        protected override string ConvertCore(IConversionContext<DateTimeOffset> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }   
}
