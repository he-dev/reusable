using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDecimalConverter : TypeConverter<String, Decimal>
    {
        protected override decimal Convert(IConversionContext<string> context)
        {
            return Decimal.Parse(context.Value, NumberStyles.Number, context.FormatProvider);
            
        }
    }

    public class DecimalToStringConverter : TypeConverter<decimal, string>
    {
        protected override string Convert(IConversionContext<decimal> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
