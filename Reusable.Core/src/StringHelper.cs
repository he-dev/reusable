using System;
using JetBrains.Annotations;

namespace Reusable
{
    public static class StringHelper
    {
        public static string Format([NotNull] FormattableString formattable, [NotNull] IFormatProvider formatProvider)
        {
            if (formattable == null) throw new ArgumentNullException(nameof(formattable));
            if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));

            return formattable.ToString(formatProvider);
        }
    }
}