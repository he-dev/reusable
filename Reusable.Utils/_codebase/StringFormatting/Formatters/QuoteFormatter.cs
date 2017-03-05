using System;
using System.Collections.Immutable;

namespace Reusable.StringFormatting.Formatters
{
    public class QuoteFormatter : CustomFormatter
    {
        public static readonly IImmutableDictionary<string, string> Quotes = ImmutableDictionary.Create<string, string>()
            .Add("double", "\"{0}\"")
            .Add("single", "'{0}'");

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return
                !string.IsNullOrEmpty(format) &&
                Quotes.TryGetValue(format, out string pattern) ? string.Format(formatProvider, pattern, arg) : null;
        }
    }
}