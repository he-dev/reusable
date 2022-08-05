using System;
using System.Drawing;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Marbles.Drawing;

namespace Reusable.Marbles.FormatProviders;

public class HexColorFormatProvider : CustomFormatProvider
{
    public HexColorFormatProvider()
        : base(new HexFormatter())
    { }

    private class HexFormatter : ICustomFormatter
    {
        // language=regexp
        //private const string FormatPattern = "(?<Prefix>0x|#|hex)?(?<ColorComponents>ARGB|RGB|RGBA)";

        [ContractAnnotation("format: null => halt; arg: null => null")]
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format is null) throw new ArgumentNullException($"'{nameof(format)}' must not be null.");
            if (arg is null) return string.Empty;

            if (arg is Color color)
            {
                var match = Regex.Match(format, "(?<alpha>a(lpha)?-)?hex", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    format = $"{(match.Groups["alpha"].Success ? "A" : string.Empty)}RGB";
                    var formatted =
                        Regex.Replace(
                            $"{(match.Groups["alpha"].Success ? "A" : string.Empty)}RGB",
                            $"(?<ColorComponent>[{(match.Groups["alpha"].Success ? "A" : string.Empty)}RGB])",
                            m => color.Component(m.Groups["ColorComponent"].Value).ToString("X2"),
                            RegexOptions.IgnoreCase | RegexOptions.Compiled
                        );

                    return $"{match.Groups["Prefix"].Value}{formatted}";
                }
            }
            return null;
        }
    }
}