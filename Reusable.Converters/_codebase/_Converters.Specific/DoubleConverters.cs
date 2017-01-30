using System;
using System.Globalization;

namespace Reusable.Converters
{
    public class StringToDoubleConverter : TypeConverter<String, Double>
    {
        protected override double ConvertCore(IConversionContext<string> context)
        {
            return Double.Parse(context.Value, NumberStyles.Float | NumberStyles.AllowThousands, context.FormatProvider);
        }
    }

    public class DoubleToStringConverter : TypeConverter<double, string>
    {
        protected override string ConvertCore(IConversionContext<double> context)
        {
            return
                string.IsNullOrEmpty(context.Format)
                    ? context.Value.ToString(context.FormatProvider)
                    : context.Value.ToString(context.Format, context.FormatProvider);
        }
    }
}
