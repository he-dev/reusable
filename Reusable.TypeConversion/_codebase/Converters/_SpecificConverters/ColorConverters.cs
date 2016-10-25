using System;
using System.Drawing;
using System.Runtime.Serialization;
using Reusable.Drawing;
using Reusable.Formatters;

namespace Reusable.Converters
{
    public class StringToColorConverter : SpecificConverter<String, Color>
    {
        private static readonly ColorParser[] ColorParsers =
        {
            new NameColorParser(),
            new DecimalColorParser(),
            new HexadecimalColorParser()
        };

        public override Color Convert(string value, ConversionContext context)
        {
            foreach (var colorParser in ColorParsers)
            {
                var argb = 0;
                if (colorParser.TryParse(value, out argb))
                {
                    return new Color32(argb);
                }
            }
            return new Color32();
        }
    }

    // ---

    public class ColorToStringConverter : SpecificConverter<Color, String>
    {
        private static readonly Formatter Formatter = Formatter.Default().Add<HexadecimalColorFormatter>();

        public override String Convert(Color value, ConversionContext context)
        {
            return string.Format(Formatter, "{0:rgb-hex}", value);
        }
    }
}
