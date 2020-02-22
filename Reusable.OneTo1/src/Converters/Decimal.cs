using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDecimal : TypeConverter<string, decimal>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public NumberStyles NumberStyles { get; set; } = NumberStyles.Number;
        
        protected override decimal Convert(string value, ConversionContext context)
        {
            return decimal.Parse(value, NumberStyles, FormatProvider);
            
        }
    }

    public class DecimalToStringConverter : TypeConverter<decimal, string>
    {
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        public string? FormatString { get; set; }
        
        protected override string Convert(decimal value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(FormatString)
                    ? value.ToString(FormatProvider)
                    : value.ToString(FormatString, FormatProvider);
        }
    }
}
