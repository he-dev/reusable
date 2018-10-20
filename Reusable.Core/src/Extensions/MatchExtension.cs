using System;
using System.Text.RegularExpressions;

namespace Reusable.Extensions
{
    public static class MatchExtension
    {
        public static Match OnFailure(this Match match, Func<Match, Exception> onFailure)
        {
            return match.Success ? match : throw onFailure(match);
        }
    }
}