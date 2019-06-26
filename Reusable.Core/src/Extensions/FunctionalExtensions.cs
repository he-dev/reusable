﻿using System;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class FunctionalExtensions
    {
        [ContractAnnotation("obj: null => null; obj: notnull => notnull")]
        public static T Next<T>([CanBeNull] this T obj, [NotNull] Action<T> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            next(obj);
            return obj;
        }

        [NotNull]
        public static T NotNull<T>([CanBeNull] this T obj) where T : class
        {
            return obj ?? throw new ArgumentNullException(nameof(obj), $"Object of type '{typeof(T).ToPrettyString()}' must not be null.");
        }

        [NotNull]
        public static T Do<T>([NotNull] this T obj, [NotNull] Action<T> next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            next(obj);
            return obj;
        }
    }
}