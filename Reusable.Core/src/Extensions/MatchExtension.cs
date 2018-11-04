using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class MatchExtension
    {
        public static Match OnFailure(this Match match, Func<Match, Exception> onFailure)
        {
            return match.Success ? match : throw onFailure(match);
        }

        public static string Group([NotNull] this Match match, string name, [CanBeNull] Func<string, string> onSuccess = null, [CanBeNull] Func<string> onFailure = null)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            return
                match.Groups[name].Success
                    ? onSuccess?.Invoke(match.Groups[name].Value) ?? match.Groups[name].Value
                    : onFailure?.Invoke();
        }
    }
}