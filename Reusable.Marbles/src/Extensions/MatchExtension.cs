using System;
using System.Text.RegularExpressions;

namespace Reusable.Marbles.Extensions;

public static class MatchExtension
{
    public static Match OnFailure(this Match match, Func<Match, Exception> onFailure)
    {
        return match.Success ? match : throw onFailure(match);
    }

    public static string Group(this Match match, string name, Func<string, string>? onSuccess = null, Func<string>? onFailure = null)
    {
        return
            match.Groups[name].Success
                ? onSuccess?.Invoke(match.Groups[name].Value) ?? match.Groups[name].Value
                : onFailure?.Invoke() ?? string.Empty;
    }
}