using System;

namespace Reusable.Formatters
{
    public class CaseFormatter : Formatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }

            if (format.Equals("u", StringComparison.OrdinalIgnoreCase))
            {
                return arg.ToString().ToUpper();
            }

            if (format.Equals("l", StringComparison.OrdinalIgnoreCase))
            {
                return arg.ToString().ToLower();
            }

            return null;
        }
    }
}