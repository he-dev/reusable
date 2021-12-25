using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToUInt16 : TypeConverter<String, UInt16>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    protected override UInt16 Convert(string value, ConversionContext context)
    {
        return UInt16.Parse(value, FormatProvider);
    }
}

public class UInt16ToStringConverter : TypeConverter<ushort, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
    protected override string Convert(ushort value, ConversionContext context)
    {
        return value.ToString(FormatProvider);
    }
}