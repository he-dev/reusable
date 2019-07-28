using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Exceptionize
{
    [PublicAPI]
    public static class DynamicExceptionExtensions
    {
        [ContractAnnotation("ex: null => halt; pattern: null => halt")]
        public static bool NameMatches([NotNull] this DynamicException ex, [NotNull, RegexPattern] string pattern)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.IsMatch(ex.GetType().Name, pattern, RegexOptions.IgnoreCase);
        }

        [ContractAnnotation("ex: null => halt; value: null => halt")]
        public static bool NameStartsWith([NotNull] this DynamicException ex, [NotNull] string value)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return ex.NameMatches($"^{value}");
        }
        
        [ContractAnnotation("ex: null => halt; value: null => halt")]
        public static bool NameContains([NotNull] this DynamicException ex, [NotNull] string value)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return ex.NameMatches(value);
        }
    }
}