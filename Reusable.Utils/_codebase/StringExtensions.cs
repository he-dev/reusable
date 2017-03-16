using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public static class StringExtensions
    {
        public static string NonEmptyOrNull(this string value) => string.IsNullOrEmpty(value) ? null : value;

        public static string NonWhitespaceOrNull(this string value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
