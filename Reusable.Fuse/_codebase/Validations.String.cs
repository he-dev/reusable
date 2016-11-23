using System;
using System.Text.RegularExpressions;

namespace Reusable.Fuse
{
    public static class StringValidation
    {
        public static ICurrent<string> IsNullOrEmpty(this ICurrent<string> current, string message = null)
        {
            return current.Check(
                value => string.IsNullOrEmpty(current.Value),
                message ?? $"\"{current.MemberName}\" must be null or empty.");
        }

        public static ICurrent<string> IsNotNullOrEmpty(this ICurrent<string> current, string message = null)
        {
            return current.Check(
                value => !string.IsNullOrEmpty(current.Value),
                message ?? $"\"{current.MemberName}\" must not be null or empty.");
        }
        
        public static ICurrent<string> IsMatch(this ICurrent<string> current, string pattern, RegexOptions options = RegexOptions.None, string message = null)
        {
            return current.Check(
                value => Regex.IsMatch(value, pattern, options),
                message ?? $"\"{current.MemberName}\" value \"({current.Value})\" must match \"{pattern}\".");
        }

        public static ICurrent<string> IsNotMatch(this ICurrent<string> current, string pattern, RegexOptions options = RegexOptions.None, string message = null)
        {
            return current.Check(
                value => !Regex.IsMatch(value, pattern, options),
                message ?? $"\"{current.MemberName}\" value \"({current.Value})\" must not match \"{pattern}\".");
        }
    }    
}
