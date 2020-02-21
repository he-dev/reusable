using System;
using System.Collections.Generic;
using System.Drawing;
using Reusable.Drawing;

namespace Reusable.OneTo1.Converters
{
    public class StringToColor : FromStringConverter<Color>
    {
        private readonly IEnumerable<ColorParser> _colorParsers;

        public StringToColor() : this(new ColorParser[] { new HexColorParser() }) { }

        public StringToColor(IEnumerable<ColorParser> colorParsers)
        {
            _colorParsers = colorParsers;
        }

        protected override Color Convert(string value, ConversionContext context)
        {
            foreach (var colorParser in _colorParsers)
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

    public class ColorToStringConverter : ToStringConverter<Color>
    {
        protected override string Convert(Color value, ConversionContext context)
        {
            return string.Format(context.FormatProvider, context.FormatString, value);
        }
    }
}