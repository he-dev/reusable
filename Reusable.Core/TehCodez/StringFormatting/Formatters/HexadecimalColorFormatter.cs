using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Drawing;

namespace Reusable.StringFormatting.Formatters
{
    public class HexadecimalColorFormatter : CustomFormatter
    {
        [ContractAnnotation("format: null => null; arg: null => null")]
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null) return null;

            if (arg is Color color)
            {
                var match = Regex.Match(format, "(0x|#)(?<ColorComponents>ARGB|RGB|RGBA)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    return Regex.Replace(
                        match.Groups["ColorComponents"].Value,
                        "(?<ColorComponent>[ARGB])",
                        m => color.Component(m.Groups["ColorComponent"].Value).ToString("X2"),
                        RegexOptions.IgnoreCase
                    );
                }
            }

            return null;
        }
    }
}