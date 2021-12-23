using System;
using System.Text.RegularExpressions;

namespace Reusable.Essentials.FormatProviders;

public class CaseFormatProvider : CustomFormatProvider
{
    public CaseFormatProvider() : base(new CaseFormatter()) { }

    private class CaseFormatter : ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is null) { return null; }
            if (format is null) { return null; }

            if (arg is string str)
            {
                if (Regex.IsMatch(format, "ToUpper|Upper|U", RegexOptions.IgnoreCase | RegexOptions.Compiled))
                {
                    return str.ToUpper();
                }

                if (Regex.IsMatch(format, "ToLower|Lower|L", RegexOptions.IgnoreCase | RegexOptions.Compiled))
                {
                    return str.ToLower();
                }
            }

            return null;
        }
    }
}