using System;
using System.Text.RegularExpressions;

namespace Reusable
{
    internal static class StringExtensions
    {
        public static bool IsNumeric(this string value) => int.TryParse(value, out int x); // !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"^\d+$");
    }
}