using System.Text.RegularExpressions;

namespace Reusable.Essentials.Extensions;

public static class MatchExtensions
{
    public static string Value(this Match match, string groupName) => match.Groups[groupName].Value;
}