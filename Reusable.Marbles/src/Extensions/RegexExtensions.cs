using System.Text.RegularExpressions;

namespace Reusable.Marbles.Extensions;

public static class RegexExtensions
{
    public static bool TryMatch(this Regex regex, string value, out Match match) => (match = regex.Match(value)).Success;
}