using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToTimeSpan : TypeConverter<String, TimeSpan>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    protected override TimeSpan Convert(string value, ConversionContext context)
    {
        return TimeSpan.Parse(value, FormatProvider);
    }
}

public class TimeSpanToString : TypeConverter<TimeSpan, String>
{
    protected override string Convert(TimeSpan value, ConversionContext context)
    {
        return value.ToString();
    }
}