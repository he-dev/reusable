using System;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Reusable.Extensions
{
    public static class Conditional
    {
        [ContractAnnotation("value: null => true; notnull => false")]
        public static bool IsNull<T>([CanBeNull] this T value) => ReferenceEquals(value, null);

        [ContractAnnotation("value: null => false; notnull => true")]
        public static bool IsNotNull<T>([CanBeNull] this T value) => !IsNull(value);

        [ContractAnnotation("value: null => true")]
        public static bool IsNullOrEmpty([CanBeNull] this string value) => string.IsNullOrEmpty(value);

        [ContractAnnotation("value: null => false")]
        public static bool IsNotNullOrEmpty([CanBeNull] this string value) => !IsNullOrEmpty(value);

        #region IIf

        public static TResult IIf<TValue, TResult>(this TValue value, [NotNull] Func<TValue, bool> predicate, [NotNull] Func<TValue, TResult> ifTrue, [NotNull] Func<TValue, TResult> ifFalse)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (ifTrue == null) throw new ArgumentNullException(nameof(ifTrue));
            if (ifFalse == null) throw new ArgumentNullException(nameof(ifFalse));

            return predicate(value) ? ifTrue(value) : ifFalse(value);
        }

        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TValue, TResult> ifTrue)
        {
            return value.IIf(predicate, ifTrue, x => default(TResult));
        }

        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TResult> ifTrue, Func<TResult> ifFalse)
        {
            return value.IIf(predicate, x => ifTrue(), x => ifFalse());
        }

        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, Func<TResult> ifTrue)
        {
            return value.IIf(predicate, x => ifTrue(), x => default(TResult));
        }

        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, TResult ifTrue, TResult ifFalse)
        {
            return value.IIf(predicate, x => ifTrue, x => ifFalse);
        }

        public static TResult IIf<TValue, TResult>(this TValue value, Func<TValue, bool> predicate, TResult ifTrue)
        {
            return value.IIf(predicate, x => ifTrue, x => default(TResult));
        }

        public static void IIf<TValue>(this TValue value, Func<TValue, bool> predicate, Action<TValue> ifTrue, Action<TValue> ifFalse)
        {
            if (predicate(value))
            {
                ifTrue(value);
            }
            else
            {
                ifFalse(value);
            }
        }

        public static void IIf<TValue>(this TValue value, Func<TValue, bool> predicate, Action<TValue> ifTrue)
        {
            value.IIf(predicate, ifTrue, x => { });
        }

        #endregion

        //public static bool IfInstanceOf<T>(this object arg, Action<T> body) where T : class
        //{
        //    if (arg == null) throw new ArgumentNullException(nameof(arg));
        //    if (body == null) throw new ArgumentNullException(nameof(body));

        //    if (arg is T argT)
        //    {
        //        body(argT);
        //        return true;
        //    }
        //    return false;
        //}
    }
}
