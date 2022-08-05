using System;

namespace Reusable.Marbles;

public static class FormattableStringHelper
{
    public static string Format(FormattableString formattable, IFormatProvider formatProvider)
    {
        if (formattable == null) throw new ArgumentNullException(nameof(formattable));
        if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));

        return formattable.ToString(formatProvider);
    }
}