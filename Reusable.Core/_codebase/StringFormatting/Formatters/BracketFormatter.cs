using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reusable.StringFormatting.Formatters
{
    // https://en.wikipedia.org/wiki/Bracket
    public class BracketFormatter : CustomFormatter
    {
        public static readonly IImmutableDictionary<string, string> Formats = ImmutableDictionary.Create<string, string>()
            .Add("round", "({0})")
            .Add("square", "[{0}]")
            .Add("curly", "{{{0}}}")
            .Add("angle", "<{0}>")
            .Add("guillemet", "«{0}»");

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return Formats.TryGetValue(format ?? string.Empty, out string pattern) ? string.Format(formatProvider, pattern, arg) : null;
        }
    }
}