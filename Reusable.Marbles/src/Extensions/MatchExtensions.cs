using System.Text.RegularExpressions;

namespace Reusable.Marbles.Extensions;

public static class MatchExtensions
{
    public static string Value(this Match match, string groupName) => match.Groups[groupName].Value;
}