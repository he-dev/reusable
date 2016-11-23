using System;
using System.Drawing;

namespace Reusable.Formatters
{
    public class DecimalColorFormatter : Formatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is Color) || string.IsNullOrEmpty(format))
            {
                return null;
            }

            var color = (Color)arg;
            var argb = color.ToArgb();

            if (format.Equals("rgb-dec", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(Culture, "({0},{1},{2})", color.R, color.G, color.B);
            }

            if (format.Equals("argb-dec", StringComparison.OrdinalIgnoreCase))
            {
                return String.Format(Culture, "({0},{1},{2},{3})", color.A, color.R, color.G, color.B);
            }

            return null;
        }
    }
}