using System;
using System.Drawing;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Drawing;

namespace Reusable.Formatters
{
    public class HexadecimalColorFormatter : CustomFormatter
    {
        // language=regexp
        private const string FormatPattern = "(?<Prefix>0x|#)?(?<ColorComponents>ARGB|RGB|RGBA)";

        [ContractAnnotation("format: null => null; arg: null => null")]
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format != null && arg is Color color)
            {
                var match = Regex.Match(format, FormatPattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    return match.Groups["Prefix"].Value + Regex.Replace(
                        match.Groups["ColorComponents"].Value,
                        "(?<ColorComponent>[ARGB])",
                        m => color.Component(m.Groups["ColorComponent"].Value).ToString("X2"),
                        RegexOptions.IgnoreCase | RegexOptions.Compiled
                    );
                }
            }

            return null;
        }
    }
}