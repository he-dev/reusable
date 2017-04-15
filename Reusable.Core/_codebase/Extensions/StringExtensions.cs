using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reusable.Extensions
{
    public static class StringExtensions
    {
        public static string NullIfEmpty(this string value) => String.IsNullOrEmpty(value) ? null : value;

        public static string NonWhitespaceOrNull(this string value) => String.IsNullOrWhiteSpace(value) ? null : value;

        public static string ExtractConnectionStringName(this string value)
        {
            if (value.IsNotNullOrEmpty()) { throw new ArgumentNullException(nameof(value)); }

            return Regex
                .Match(value, @"\Aname=(?<name>.+)", RegexOptions.IgnoreCase)
                .Groups["name"]
                .Value;
        }     
    }
}
