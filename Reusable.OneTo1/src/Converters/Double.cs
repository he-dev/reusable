using System;
using System.Globalization;

namespace Reusable.OneTo1.Converters
{
    public class StringToDouble : TypeConverter<string, double>
    {
        protected override double Convert(string value, ConversionContext context)
        {
            return double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, context.FormatProvider);
        }
    }

    public class DoubleToStringConverter : TypeConverter<double, string>
    {
        protected override string Convert(double value, ConversionContext context)
        {
            return
                string.IsNullOrEmpty(context.FormatString)
                    ? value.ToString(context.FormatProvider)
                    : value.ToString(context.FormatString, context.FormatProvider);
        }
    }
}