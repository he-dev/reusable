using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Reusable.Essentials.Drawing;

namespace Reusable.Snowball.Converters;

public class StringToColor : TypeConverter<string, Color>
{
    public List<ColorParser> ColorParsers { get; set; } = new List<ColorParser>
    {
        new HexColorParser()
    };

    protected override Color Convert(string value, ConversionContext context)
    {
        foreach (var colorParser in ColorParsers)
        {
            if (colorParser.TryParse(value, out var argb))
            {
                return new Color32(argb);
            }
        }

        return Color.Empty;
    }
}

// ---

public class ColorToStringConverter : TypeConverter<Color, string>
{
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    public string? FormatString { get; set; }
        
    protected override string Convert(Color value, ConversionContext context)
    {
        return string.Format(FormatProvider, FormatString, value);
    }
}