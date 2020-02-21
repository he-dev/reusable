using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDecimal : TypeConverter<string, decimal>
    {
        protected override decimal Convert(string value, ConversionContext context)
        {
            return decimal.Parse(value, NumberStyles.Number, context.FormatProvider);
            
        }
    }

    public class DecimalToStringConverter : TypeConverter<decimal, string>
    {
        protected override string Convert(decimal value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(context.FormatString)
                    ? value.ToString(context.FormatProvider)
                    : value.ToString(context.FormatString, context.FormatProvider);
        }
    }
}
