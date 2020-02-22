using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDateTimeOffset : TypeConverter<string, DateTimeOffset>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public string? FormatString { get; set; }
        
        protected override DateTimeOffset Convert(string value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(FormatString)
                    ? DateTimeOffset.Parse(value, FormatProvider, DateTimeStyles.None)
                    : DateTimeOffset.ParseExact(value, FormatString, FormatProvider, DateTimeStyles.None);
        }
    }

    public class DateTimeOffsetToString : TypeConverter<DateTimeOffset, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public string? FormatString { get; set; }
        
        protected override string Convert(DateTimeOffset value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(FormatString)
                    ? value.ToString(FormatProvider)
                    : value.ToString(FormatString, FormatProvider);
        }
    }   
}
