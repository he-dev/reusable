using System;
using System.Globalization;

namespace Reusable.Snowball.Converters;

public class StringToGuidConverter : TypeConverter<String, Guid>
{
    protected override Guid Convert(string value, ConversionContext context)
    {
        return Guid.Parse(value);
    }
}

public class GuidToStringConverter : TypeConverter<Guid, String>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public string? FormatString { get; set; }
        
    protected override string Convert(Guid value, ConversionContext context)
    {
        return
            string.IsNullOrEmpty(FormatString)
                ? value.ToString()
                : value.ToString(FormatString, FormatProvider);
    }
}