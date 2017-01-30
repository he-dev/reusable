using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Reusable.Drawing;

namespace Reusable.Converters
{
    public class StringToColorConverter : TypeConverter<String, Color>
    {
        private readonly IEnumerable<ColorParser> _colorParsers;

        public StringToColorConverter() : this(new ColorParser[] { new HexadecimalColorParser() }) { }

        public StringToColorConverter(IEnumerable<ColorParser> colorParsers)
        {
            _colorParsers = colorParsers;
        }

        protected override Color ConvertCore(IConversionContext<String> context)
        {
            foreach (var colorParser in _colorParsers)
            {
                var argb = 0;
                if (colorParser.TryParse(context.Value, out argb))
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
        protected override String ConvertCore(IConversionContext<Color> context)
        {
            return string.Format(context.FormatProvider, context.Format, context.Value);
        }
    }
}
