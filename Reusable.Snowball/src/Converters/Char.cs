using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToChar : TypeConverter<string, char>
{
    protected override char Convert(string value, ConversionContext context)
    {
        return char.Parse(value);
    }
}

public class CharToStringConverter : TypeConverter<char, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
    protected override string Convert(char value, ConversionContext context)
    {
        return value.ToString(FormatProvider);
    }
}