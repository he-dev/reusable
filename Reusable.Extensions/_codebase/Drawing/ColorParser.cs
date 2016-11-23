using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Reusable.Drawing
{
    public abstract class ColorParser
    {
        public int Parse(string value)
        {
            int result;
            if (!TryParse(value, out result))
            {
                throw new FormatException($"Unknown color format: '{value}'");
            }
            return result;
        }
        public abstract bool TryParse(string value, out int color);
    }

    public class DecimalColorParser : ColorParser
    {
        public override bool TryParse(string value, out int color)
        {
            value = Regex.Replace(value, @"\s", string.Empty);

            const string colorPattern = @"\d{1,2}|[1][0-9][0-9]|[2][0-5][0-5]";
            const string delimiterPattern = @"[,;:]";
            var pattern = string.Format(@"^(?:(?<A>{0}){1})?(?<R>{0}){1}(?<G>{0}){1}(?<B>{0})$", colorPattern, delimiterPattern);

            var match = Regex.Match(value, pattern);
            if (!match.Success)
            {
                color = 0;
                return false;
            }

            var alpha = match.Groups["A"].Success ? int.Parse(match.Groups["A"].Value) : 255;
            var red = int.Parse(match.Groups["R"].Value);
            var green = int.Parse(match.Groups["G"].Value);
            var blue = int.Parse(match.Groups["B"].Value);

            color = Color.FromArgb(alpha, red, green, blue).ToArgb();
            return true;
        }
    }

    public class HexadecimalColorParser : ColorParser
    {
        public override bool TryParse(string value, out int color)
        {
            value = Regex.Replace(value, @"\s", string.Empty);

            var match = Regex.Match(value, @"^#?(?<A>[A-F0-9]{2})?(?<R>[A-F0-9]{2})(?<G>[A-F0-9]{2})(?<B>[A-F0-9]{2})$", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                color = 0;
                return false;
            }

            var alpha = match.Groups["A"].Success ? int.Parse(match.Groups["A"].Value, NumberStyles.HexNumber) : 255;
            var red = int.Parse(match.Groups["R"].Value, NumberStyles.HexNumber);
            var green = int.Parse(match.Groups["G"].Value, NumberStyles.HexNumber);
            var blue = int.Parse(match.Groups["B"].Value, NumberStyles.HexNumber);

            color = Color.FromArgb(alpha, red, green, blue).ToArgb();
            return true;
        }
    }

    public class NameColorParser : ColorParser
    {
        public override bool TryParse(string value, out int color)
        {
            value = Regex.Replace(value, @"\s", string.Empty);

            var colorFromName = Color.FromName(value);

            if (!colorFromName.IsKnownColor)
            {
                color = 0;
                return false;
            }

            color = colorFromName.ToArgb();
            return true;
        }
    }
}
