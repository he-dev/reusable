using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Essentials.Drawing;

namespace Reusable.Essentials.FormatProviders;

public class RgbColorFormatProvider : CustomFormatProvider
{
    public RgbColorFormatProvider() :
        base(new RgbColorFormatter())
    { }

    private class RgbColorFormatter : ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format is null) throw new ArgumentNullException($"'{nameof(format)}' must not be null.");
            if (arg is null) return string.Empty;

            if (arg is Color color)
            {
                var match = Regex.Match(format, "(?<ColorComponents>ARGB|RGB|RGBA)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
                if (match.Success)
                {
                    format = string.Join(", ", match.Groups["ColorComponents"].Value.AsEnumerable());

                    return
                        Regex.Replace(
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