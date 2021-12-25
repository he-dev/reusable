using System;
using System.Globalization;
using Reusable.Essentials;

namespace Reusable.Snowball.Converters;

public class StringToDateTime : TypeConverter<string, DateTime>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public string? FormatString { get; set; }
        
    protected override DateTime Convert(string value, ConversionContext context)
    {
        return
            FormatString.IsNullOrEmpty()
                ? DateTime.Parse(value, FormatProvider, DateTimeStyles.None)
                : DateTime.ParseExact(value, FormatString, FormatProvider, DateTimeStyles.None);
    }
}

public class DateTimeToString : TypeConverter<DateTime, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public string? FormatString { get; set; }
        
    protected override string Convert(DateTime value, ConversionContext context)
    {
        return
            FormatString.IsNullOrEmpty()
                ? value.ToString(FormatProvider)
                : value.ToString(FormatString, FormatProvider);
    }
}