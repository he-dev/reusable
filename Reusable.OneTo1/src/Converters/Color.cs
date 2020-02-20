using System;
using System.Collections.Generic;
using System.Drawing;
using Reusable.Drawing;

namespace Reusable.OneTo1.Converters
{
    public class StringToColorConverter : TypeConverter<String, Color>
    {
        private readonly IEnumerable<ColorParser> _colorParsers;

        public StringToColorConverter() : this(new ColorParser[] { new HexColorParser() }) { }

        public StringToColorConverter(IEnumerable<ColorParser> colorParsers)
        {
            _colorParsers = colorParsers;
        }

        protected override Color Convert(IConversionContext<String> context)
        {
            foreach (var colorParser in _colorParsers)
            {
                if (colorParser.TryParse(context.Value, out var argb))
                {
                    return new Color32(argb);
                }
            }
            return Color.Empty;
        }
    }

    // ---

    public class ColorToStringConverter : TypeConverter<Color, String>
    {
        protected override String Convert(IConversionContext<Color> context)
        {
            return string.Format(context.FormatProvider, context.Format, context.Value);
        }
    }
}
