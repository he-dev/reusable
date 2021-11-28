﻿using System;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class StringBuilderExtensions
    {
        public static bool Any(this StringBuilder stringBuilder) => stringBuilder.Length > 0;

        public static StringBuilder AppendWhen([NotNull] this StringBuilder stringBuilder, [NotNull] Func<bool> canAppend, [NotNull] Action<StringBuilder> append)
        {
            if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
            if (canAppend == null) throw new ArgumentNullException(nameof(canAppend));
            if (append == null) throw new ArgumentNullException(nameof(append));

            return canAppend() ? stringBuilder.Also(append) : stringBuilder;
        }

        public static StringBuilder AppendWhen([NotNull] this StringBuilder stringBuilder, bool canAppend, [NotNull] Func<string> getValue)
        {
            if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));
            return canAppend ? stringBuilder.Append(getValue()) : stringBuilder;
        }

        public static StringBuilder AppendWhen<T>([NotNull] this StringBuilder stringBuilder, [NotNull] Func<T> getValue, [NotNull] Func<T, bool> canAppend, Action<StringBuilder, T> append)
        {
            if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));
            if (canAppend == null) throw new ArgumentNullException(nameof(canAppend));

            var value = getValue();
            return canAppend(value) ? stringBuilder.Also(sb => append(sb, value)) : stringBuilder;
        }
        
        [NotNull]
        public static StringBuilder TrimEnd([NotNull] this StringBuilder stringBuilder, [NotNull] string value)
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
}
