using System;
using System.Text;

namespace Reusable.Extensions;

public static class StringBuilderExtensions
{
    public static bool Any(this StringBuilder stringBuilder) => stringBuilder.Length > 0;

    public static StringBuilder AppendWhen(this StringBuilder stringBuilder, Func<bool> canAppend, Action<StringBuilder> append)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
        if (canAppend == null) throw new ArgumentNullException(nameof(canAppend));
        if (append == null) throw new ArgumentNullException(nameof(append));

        return canAppend() ? stringBuilder.Also(append) : stringBuilder;
    }

    public static StringBuilder AppendWhen(this StringBuilder stringBuilder, bool canAppend, Func<string> getValue)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        return canAppend ? stringBuilder.Append(getValue()) : stringBuilder;
    }

    public static StringBuilder AppendWhen<T>(this StringBuilder stringBuilder, Func<T> getValue, Func<T, bool> canAppend, Action<StringBuilder, T> append)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        if (canAppend == null) throw new ArgumentNullException(nameof(canAppend));

        var value = getValue();
        return canAppend(value) ? stringBuilder.Also(sb => append(sb, value)) : stringBuilder;
    }
        
    public static StringBuilder TrimEnd(this StringBuilder stringBuilder, string value)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (stringBuilder.Length < value.Length) return stringBuilder;

        var startIndex = stringBuilder.Length - value.Length;
        for (int i = startIndex, j = 0; j < value.Length; i++, j++)
        {
            if (!stringBuilder[i].Equals(value[j]))
            {
                return stringBuilder;
            }
        }

        return 
            stringBuilder
                .Remove(startIndex, value.Length)
                .TrimEnd(value);
    }
}