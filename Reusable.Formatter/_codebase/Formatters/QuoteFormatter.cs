using System;

namespace Reusable.Formatters
{
    public class QuoteFormatter : Formatter
    {
        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }

            if (format.Equals("dq", StringComparison.OrdinalIgnoreCase))
            {
                return $"\"{arg}\"";
            }

            if (format.Equals("sq", StringComparison.OrdinalIgnoreCase))
            {
                return $"'{arg}'";
            }

            return null;
           
        }
    }
}