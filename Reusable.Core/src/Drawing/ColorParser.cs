using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Drawing
{
    [UsedImplicitly]
    public abstract class ColorParser
    {
        protected const int Empty = 0;

        [ContractAnnotation("value: null => halt")]
        public int Parse([NotNull] string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (TryParse(value, out int result))
            {
                return result;
            }

            throw new FormatException($"Unknown color format: '{value}'");
        }

        [ContractAnnotation("value: null => false")]
        public bool TryParse([CanBeNull] string value, out int color)
        {
            if (value == null)
            {
                color = Empty;
                return false;
            }
            return TryParseCore(value, out color);
        }

        protected abstract bool TryParseCore([NotNull] string value, out int color);

        // Removes all spaces from a string.
        [ContractAnnotation("value: null => null; notnull => notnull")]
        protected static string Minify([CanBeNull] string value)
        {
            return value == null ? null : Regex.Replace(value, @"\s", string.Empty);
        }
    }

    public class RgbColorParser : ColorParser
    {
        // language=regexp
        private const string ColorPattern = @"\d{1,2}|[1][0-9][0-9]|[2][0-5][0-5]";

        // language=regexp
        private const string DelimiterPattern = @"[,;:]";

        // language=regexp
        private readonly string _argbPattern =
            // Using $ everywhere for consistency.
            $"^(?:" +
            $"(?<A>{ColorPattern}){DelimiterPattern})?" +
            $"(?<R>{ColorPattern}){DelimiterPattern}" +
            $"(?<G>{ColorPattern}){DelimiterPattern}" +
            $"(?<B>{ColorPattern}" +
            $")$";

        protected override bool TryParseCore(string value, out int color)
        {
            var match = Regex.Match(Minify(value), _argbPattern, RegexOptions.ExplicitCapture);
            if (match.Success)
            {
                var alpha = match.Groups["A"].Success ? int.Parse(match.Groups["A"].Value) : 255;
                var red = int.Parse(match.Groups["R"].Value);
                var green = int.Parse(match.Groups["G"].Value);
                var blue = int.Parse(match.Groups["B"].Value);

                color = Color.FromArgb(alpha, red, green, blue).ToArgb();
                return true;
            }

            color = Empty;
            return false;
        }
    }

    public class HexColorParser : ColorParser
    {
        // language=regexp
        private const string ColorPattern = @"^(0x|#)?(?<A>[A-F0-9]{2})?(?<R>[A-F0-9]{2})(?<G>[A-F0-9]{2})(?<B>[A-F0-9]{2})$";

        protected override bool TryParseCore(string value, out int color)
        {
            var match = Regex.Match(Minify(value), ColorPattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            if (match.Success)
            {
                var alpha = match.Groups["A"].Success ? int.Parse(match.Groups["A"].Value, NumberStyles.HexNumber) : 255;
                var red = int.Parse(match.Groups["R"].Value, NumberStyles.HexNumber);
                var green = int.Parse(match.Groups["G"].Value, NumberStyles.HexNumber);
                var blue = int.Parse(match.Groups["B"].Value, NumberStyles.HexNumber);

                color = Color.FromArgb(alpha, red, green, blue).ToArgb();
                return true;
            }

            color = Empty;
            return false;
        }
    }

    public class NameColorParser : ColorParser
    {
        protected override bool TryParseCore(string value, out int color)
        {
            var colorFromName = Color.FromName(Minify(value));

            if (colorFromName.IsKnownColor)
            {
                color = colorFromName.ToArgb();
                return true;
            }

            color = Empty;
            return false;
        }
    }
}
