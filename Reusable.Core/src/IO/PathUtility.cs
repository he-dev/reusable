using System.Text.RegularExpressions;
using Reusable.Extensions;

namespace Reusable.IO
{
    public static class PathUtility
    {
        public static bool WildcardMatches(this string path, string pattern)
        {
            pattern = pattern.RegexReplace(@"\\", @"\\");
            pattern = pattern.RegexReplace(@"\.", @"\.");
            pattern = pattern.RegexReplace(@"\?", @".");
            pattern = pattern.RegexReplace(@"\*", @".*?");

            return Regex.IsMatch(path, $"^{pattern}$", RegexOptions.IgnoreCase);
        }
    }
}