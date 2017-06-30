using System.Text.RegularExpressions;

namespace Reusable.SmartConfig.Extensions
{
    public static class StringExtensions
    {
        public static string TrimEnd(this string input, string pattern, bool ignoreCase = false)
        {
            return Regex.Replace(input, $"{pattern}$", string.Empty, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
    }
}