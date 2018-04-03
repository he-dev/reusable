using System;
using System.Collections.Generic;

namespace Reusable.Formatters
{
    // https://en.wikipedia.org/wiki/Bracket
    public class BracketFormatter : CustomFormatter
    {
        public static readonly IReadOnlyDictionary<string, string> Formats = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["round"] = "({0})",
            ["square"] = "[{0}]",
            ["curly"] = "{{{0}}}",
            ["angle"] = "<{0}>",
            ["guillemet"] = "«{0}»"
        };

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return Formats.TryGetValue(format ?? string.Empty, out var pattern) ? string.Format(formatProvider, pattern, arg) : null;
        }
    }
}