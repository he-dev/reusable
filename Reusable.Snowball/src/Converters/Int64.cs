using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToInt64 : TypeConverter<String, Int64>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public NumberStyles NumberStyles { get; set; } = NumberStyles.Integer;
        
    protected override long Convert(string value, ConversionContext context)
    {
        return Int64.Parse(value, NumberStyles, FormatProvider);
    }
}

public class Int64ToStringConverter : TypeConverter<long, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    protected override string Convert(long value, ConversionContext context)
    {
        return value.ToString(FormatProvider);
    }
}