using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToDouble : TypeConverter<string, double>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public NumberStyles NumberStyles { get; set; } = NumberStyles.Float | NumberStyles.AllowThousands;

    protected override double Convert(string value, ConversionContext context)
    {
        return double.Parse(value, NumberStyles, FormatProvider);
    }
}

public class DoubleToStringConverter : TypeConverter<double, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public string? FormatString { get; set; }
        
    protected override string Convert(double value, ConversionContext context)
    {
        return
            string.IsNullOrEmpty(FormatString)
                ? value.ToString(FormatProvider)
                : value.ToString(FormatString, FormatProvider);
    }
}