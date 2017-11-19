using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class MatchExtensions
    {
        [NotNull, ContractAnnotation("match: null => halt; groupName: null => halt")]
        public static string Value([NotNull] this Match match, [NotNull] string groupName)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            if (groupName == null) throw new ArgumentNullException(nameof(groupName));

            return match.Groups[groupName].Value;
        }
    }
}