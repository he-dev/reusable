using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToInt32 : TypeConverter<String, Int32>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public NumberStyles NumberStyles { get; set; } = NumberStyles.Integer;
        
    protected override int Convert(string value, ConversionContext context)
    {
        return Int32.Parse(value, NumberStyles, FormatProvider);
    }
}

public class Int32ToString : TypeConverter<Int32, String>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
    protected override string Convert(Int32 value, ConversionContext context)
    {
        return value.ToString(FormatProvider);
    }
}