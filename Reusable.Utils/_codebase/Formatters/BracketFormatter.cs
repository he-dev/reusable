using System;
using System.Collections.Generic;

namespace Reusable.Formatters
{
    public class BracketFormatter : CustomFormatter
    {
        private static readonly Dictionary<string, Func<object, string>> FormatFuncs = new Dictionary<string, Func<object, string>>
        {
            ["[]"] = s => $"[{s}]",
            ["{}"] = s => $"{{{s}}}",
            ["()"] = s => $"({s})"
        };
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }

            Func<object, string> formatFunc;
            return FormatFuncs.TryGetValue(format, out formatFunc) ? formatFunc(arg) : null;
        }
    }
}