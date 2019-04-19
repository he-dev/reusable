using System;
using System.Collections.Generic;

namespace Reusable.FormatProviders
{
    public class PunctuationFormatProvider : CustomFormatProvider
    {
        public PunctuationFormatProvider() : base(new PunctuationFormatter()) { }

        // https://en.wikipedia.org/wiki/Bracket
        private class PunctuationFormatter : ICustomFormatter
        {
            private static readonly IReadOnlyDictionary<string, string> Punctuations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["round"] = "()",
                ["square"] = "[]",
                ["curly"] = "{}",
                ["angle"] = "<>",
                ["guillemet"] = "«»",
                ["double"] = "\"\"",
                ["single"] = "''",
                ["backtick"] = "``",
                ["citation"] = "“ ”"
            };

            private const int Opening = 0;
            private const int Closing = 1;

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                //if (format is null) throw new ArgumentNullException($"'{nameof(format)}' must not be null.");
                if (arg is null) return string.Empty;

                return
                    arg is string str && Punctuations.TryGetValue(format ?? string.Empty, out var pattern) 
                        ? string.Format(formatProvider, $"{pattern[Opening]}{{0}}{pattern[Closing]}", arg) 
                        : null;
            }
        }
    }
}