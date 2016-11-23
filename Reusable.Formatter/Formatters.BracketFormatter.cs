using System;

namespace Reusable.Formatters
{
    public class BracketFormatter : Formatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }

            if (format.Equals("sb", StringComparison.OrdinalIgnoreCase))
            {
                return $"[{arg}]";
            }           

            return null;
        }
    }
}