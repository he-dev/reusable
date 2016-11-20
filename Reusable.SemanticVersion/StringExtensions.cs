using System.Text.RegularExpressions;

namespace Reusable
{
    internal static class StringExtensions
    {
        public static bool IsNumeric(this string value)
            => !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"^\d+$");
    }
}