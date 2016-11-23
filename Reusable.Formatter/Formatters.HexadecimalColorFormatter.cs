using System;
using System.Drawing;

namespace Reusable.Formatters
{
    public class HexadecimalColorFormatter : Formatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is Color) || string.IsNullOrEmpty(format))
            {
                return null;
            }

            var color = (Color)arg;

            if (format.Equals("rgb-hex", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(Culture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
            }

            if (format.Equals("argb-hex", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(Culture, "#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
            }

            return null;
        }
    }
}