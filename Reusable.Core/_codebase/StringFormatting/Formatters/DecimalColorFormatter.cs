using Reusable.codebase.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.StringFormatting.Formatters
{
    public class DecimalColorFormatter : CustomFormatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!string.IsNullOrEmpty(format) && arg is Color color)
            {
                var match = Regex.Match(format, "(?<ColorComponents>ARGB|RGB|RGBA)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    format = string.Join(", ", match.Groups["ColorComponents"].Value.AsEnumerable());

                    return Regex.Replace(
                        format,
                        "(?<ColorComponent>[ARGB])",
                        m => color.Component(m.Groups["ColorComponent"].Value).ToString(),
                        RegexOptions.IgnoreCase
                    );
                }
            }

            return null;
        }
    }
}