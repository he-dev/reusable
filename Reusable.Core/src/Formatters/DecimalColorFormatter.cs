using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Drawing;

namespace Reusable.Formatters
{
    public class DecimalColorFormatter : CustomFormatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!string.IsNullOrEmpty(format) && arg is Color color)
            {
                var match = Regex.Match(format, "(?<ColorComponents>ARGB|RGB|RGBA)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
                if (match.Success)
                {
                    format = string.Join(", ", match.Groups["ColorComponents"].Value.AsEnumerable());

                    return Regex.Replace(
                        format,
                        "(?<ColorComponent>[ARGB])",
                        m => color.Component(m.Groups["ColorComponent"].Value).ToString(),
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );
                }
            }

            return null;
        }
    }
}