using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToByte : TypeConverter<string, byte>
{
    protected override byte Convert(string value, ConversionContext context)
    {
        return byte.Parse(value);
    }
}

public class ByteToString : TypeConverter<byte, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;
        
    protected override string Convert(byte value, ConversionContext context)
    {
        return value.ToString(FormatProvider);
    }
}