using System;
using System.Text.RegularExpressions;

namespace Reusable.Formatters
{
    public class CaseFormatter : CustomFormatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }

            if (Regex.IsMatch(format, "ToUpper|Upper|U", RegexOptions.IgnoreCase))
            {
                return arg.ToString().ToUpper();
            }

            if (Regex.IsMatch(format, "ToLower|Lower|L", RegexOptions.IgnoreCase))
            {
                return arg.ToString().ToLower();
            }

            return null;
        }
    }
}