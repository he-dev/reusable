using System;
using System.Collections.Generic;
using Reusable.Extensions;

namespace Reusable.Formatters
{
    public class QuoteFormatter : CustomFormatter
    {
        public static readonly IReadOnlyDictionary<string, Func<string, string>> Quotes = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["double"] = s => s.QuoteWith('"'),
            ["single"] = s => s.QuoteWith('\''),
            ["backtick"] = s => s.QuoteWith('`')
        };

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return Quotes.TryGetValue(format ?? string.Empty, out var quoteWith) ? quoteWith(arg.ToString()) : null;
        }
    }
}