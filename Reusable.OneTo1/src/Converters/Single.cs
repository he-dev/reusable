using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToSingle : TypeConverter<String, Single>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        protected override Single Convert(string value, ConversionContext context)
        {
            return Single.Parse(value, FormatProvider);
        }
    }

    public class SingleToStringConverter : TypeConverter<float, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public string? FormatString { get; set; }
        
        protected override string Convert(float value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(FormatString)
                    ? value.ToString(FormatProvider)
                    : value.ToString(FormatString, FormatProvider);
        }
    }
}