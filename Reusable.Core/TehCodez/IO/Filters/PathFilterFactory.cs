using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.IO
{
    public abstract class PathFilterFactory : IFilterFactory<string, string>
    {
        // Using just a few symbols Windows is showing when renaming files.
        public static readonly string InvalidFileNameChars = @"""<>|:*?\/";

        public static Func<string, bool> None => _ => false;

        public static Func<string, bool> All => _ => true;

        public Func<string, bool> Create(string pattern)
        {
            if (IsRegex(pattern))
            {
                return new Regex(pattern.TrimStart('/').TrimEnd('/'), RegexOptions.IgnoreCase).IsMatch;
            }

            if (IsWildcards(pattern))
            {
                return new Regex(FromWildcards(pattern), RegexOptions.IgnoreCase).IsMatch;
            }

            return FromString(pattern);
        }

        protected virtual string FromWildcards(string pattern)
        {
            pattern = Regex.Replace(pattern, @"\.", @"\.");
            pattern = Regex.Replace(pattern, @"\?", @".");
            // Hard to decide which one to use.
            //result = Regex.Replace(result, @"\*", $@"[^{Regex.Escape(InvalidFileNameChars)}]*?");
            pattern = Regex.Replace(pattern, @"\*", $@".*?");
            return pattern;
        }

        protected abstract Func<string, bool> FromString(string pattern);

        private static bool IsRegex(string value)
        {
            return
                value.StartsWith("/") &&
                value.EndsWith("/");
        }

        private static bool IsWildcards(string value)
        {
            return
                value.Contains('*') ||
                value.Contains('?');
        }
    }
}