using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.StringFormatting.Formatters
{
    public class DecimalColorFormatter : CustomFormatter
    {
        private static readonly string[] Formats = { "ARGB", "RGB", "RGBA" };

        private static readonly Dictionary<string, Func<Color, byte>> ColorMap = new Dictionary<string, Func<Color, byte>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Color.A)] = c => c.A,
            [nameof(Color.R)] = c => c.R,
            [nameof(Color.G)] = c => c.G,
            [nameof(Color.B)] = c => c.B,
        };

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is Color) || string.IsNullOrEmpty(format))
            {
                return null;
            }

            if (!Formats.Contains(format, StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }

            format = string.Join(", ", format.AsEnumerable());

            var color = (Color)arg;
            return Regex.Replace(
                format,
                "(?<color>[ARGB])",
                m => ColorMap[m.Groups["color"].Value](color).ToString(),
                RegexOptions.IgnoreCase
            );
        }
    }
}